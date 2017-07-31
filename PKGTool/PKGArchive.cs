using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HedgeLib.IO;

namespace HedgeLib.Archives
{
    public class PKGArchive : Archive
    {
        public override void Load(Stream fileStream)
        {
            var reader = new ExtendedBinaryReader(fileStream, false);

            reader.JumpAhead(4); // CRC32?
            int fileCount = reader.ReadInt32();

            var fileEntries = new List<FileEntry>();
            for (int i = 0; i < fileCount; ++i)
            {
                var fileEntry = new FileEntry();
                fileEntry.FileName = reader.ReadSignature(0x40).Replace("\0", "");
                fileEntry.DataUncompressedSize = reader.ReadUInt32();
                fileEntry.DataSize = reader.ReadUInt32();
                fileEntry.DataOffset = reader.ReadUInt32();
                fileEntry.Compressed = reader.ReadByte() == 1;
                reader.JumpAhead(3); // Other Attributes?
                fileEntries.Add(fileEntry);
            }

            foreach (var fileEntry in fileEntries)
            {
                reader.JumpTo(fileEntry.DataOffset);

                byte[] data = null;
                if (fileEntry.Compressed)
                    data = ReadAndDecompress(reader);
                else
                    data = reader.ReadBytes((int)fileEntry.DataUncompressedSize);

                // Adds the File
                Data.Add(new ArchiveFile()
                {
                    Name = fileEntry.FileName,
                    Data = data
                });
            }

        }

        public override void Save(Stream fileStream)
        {
            var writer = new ExtendedBinaryWriter(fileStream, false);

            // TODO
            writer.AddOffset("crc32"); // CRC32?
            writer.Write(Data.Count);

            var fileDatas = new List<byte[]>();
            int i = 0;
            foreach (ArchiveFile file in Data)
            {
                var fileNameBuffer = new char[0x40];
                file.Name.CopyTo(0, fileNameBuffer, 0, file.Name.Length);
                writer.Write(fileNameBuffer);

                var fileData = file.Data;
                writer.Write(fileData.Length);      // Uncompressed Size
                writer.Write(fileData.Length);      // Compressed Size
                writer.AddOffset("fileOffset" + i); // File Offset
                writer.Write(0u);                   // Is Compressed 0 = FALSE, 1 = TRUE
                fileDatas.Add(fileData);
                ++i;
            }

            for (i = 0; i < fileDatas.Count; ++i)
            {
                writer.FillInOffset("fileOffset" + i);
                writer.Write(fileDatas[i]);
            }

            // TODO
            writer.FillInOffset("crc32", 0u);

        }

        public static byte[] ReadAndDecompress(ExtendedBinaryReader reader)
        {
            var stream = new MemoryStream();
            uint decompressedSize = reader.ReadUInt32();
            uint compressedSize = reader.ReadUInt32();
            byte copyByte = reader.ReadByte();
            reader.JumpAhead(3);
            uint dataStartOffset = (uint)reader.BaseStream.Position;

            while (stream.Position < decompressedSize)
            {
                byte b = reader.ReadByte();

                if (b == copyByte)
                {
                    byte returnByte = reader.ReadByte();

                    if (returnByte == copyByte)
                    {
                        stream.WriteByte(returnByte);
                        continue;
                    }

                    if (returnByte >= copyByte)
                        returnByte--;

                    uint offset = (uint)stream.Position - returnByte;
                    byte length = reader.ReadByte();
                    uint currentPosition = (uint)stream.Position;

                    stream.Position = offset;
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);
                    stream.Position = currentPosition;
                    stream.Write(buffer, 0, length);
                }
                else
                    stream.WriteByte(b);
            }
            return stream.ToArray();
        }

        public class FileEntry
        {
            public string FileName;
            public uint DataSize;
            public uint DataUncompressedSize;
            public uint DataOffset;
            public bool Compressed;
        }
    }
}

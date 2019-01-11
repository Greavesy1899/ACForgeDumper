using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACForgeDump
{
    struct IndexEntry
    {
        public ulong StartPos;
        public int Unk01;
        public int Unk02;
        public int Size;

        public override string ToString()
        {
            return string.Format("StartPos {0}, Unk01 {1}, Unk02 {2}, Size {3}", StartPos, Unk01, Unk02, Size);
        }
    }
    struct NameEntry
    {
        public int Size;
        public long FileID;
        public byte[] Unk01;
        public int NextFileCount;
        public int PreviousFileCount;
        public int Unk02;
        public int Unk03;
        public string Name;
        public byte[] Unk04;
        public byte[] FileData;

        public override string ToString()
        {
            return string.Format("Size {0}, FileID {1}, Name {2}", Size, FileID, Name);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IndexEntry[] indexEntries;
            NameEntry[] nameEntries;

            using (BinaryReader reader = new BinaryReader(File.Open(args[0], FileMode.Open)))
            {
                //not scimitar
                if (reader.ReadInt64() != 8241996789220729715)
                    return;

                //unused byte
                reader.ReadByte();
                
                //read and then dump header.
                int forgeVersion = reader.ReadInt32();
                ulong headerLength = reader.ReadUInt64();
                ulong unk0 = reader.ReadUInt64();
                int unk1 = reader.ReadInt16();
                Console.WriteLine("Forge Version: {0}", forgeVersion);
                Console.WriteLine("Header Length: {0}", headerLength);
                Console.WriteLine("Unk0: {0}", unk0);
                Console.WriteLine("Unk1: {0}", unk1);

                //know we begin.
                reader.BaseStream.Seek((long)headerLength, SeekOrigin.Begin);

                int numEntries = reader.ReadInt32();
                indexEntries = new IndexEntry[numEntries];
                reader.BaseStream.Seek(24, SeekOrigin.Current);

                if (numEntries != reader.ReadInt32())
                    Console.WriteLine("Possible issue with num entries");

                if (reader.ReadInt32() != 1)
                    Console.WriteLine("Possible issue");

                ulong nameTablePosOffset = reader.ReadUInt64();

                if (numEntries != reader.ReadInt32())
                    Console.WriteLine("Possible issue with num entries");

                if (reader.ReadInt32() != 1)
                    Console.WriteLine("Possible issue");

                ulong indexTableOffset = reader.ReadUInt64();
                reader.BaseStream.Seek(16, SeekOrigin.Current);
                ulong nameTableOffset = reader.ReadUInt64();

                reader.BaseStream.Seek((long)indexTableOffset, SeekOrigin.Begin);

                nameEntries = new NameEntry[numEntries];

                for (int i = 0; i != numEntries; i++)
                {
                    IndexEntry entry;
                    entry.StartPos = reader.ReadUInt64();
                    entry.Unk01 = reader.ReadInt32();
                    entry.Unk02 = reader.ReadInt32();
                    entry.Size = reader.ReadInt32();
                    indexEntries[i] = entry;
                }

                for (int i = 0; i != numEntries; i++)
                {
                    NameEntry entry;
                    entry.Size = reader.ReadInt32();
                    entry.FileID = reader.ReadInt64();
                    entry.Unk01 = reader.ReadBytes(4 * 4);
                    entry.NextFileCount = reader.ReadInt32();
                    entry.PreviousFileCount = reader.ReadInt32();
                    entry.Unk02 = reader.ReadInt32();
                    entry.Unk03 = reader.ReadInt32();
                    entry.Name = new string(reader.ReadChars(128));
                    entry.Name = entry.Name.Trim(new char[] { '\0' });
                    entry.Unk04 = reader.ReadBytes(4 * 5);
                    entry.FileData = null;
                    nameEntries[i] = entry;
                }

                for (int i = 0; i != numEntries; i++)
                {
                    reader.BaseStream.Seek((long)indexEntries[i].StartPos, SeekOrigin.Begin);
                    nameEntries[i].FileData = reader.ReadBytes(nameEntries[i].Size);

                    using (BinaryWriter writer = new BinaryWriter(File.Open(nameEntries[i].Name + ".dat", FileMode.Create)))
                    {
                        writer.Write(nameEntries[i].FileData);
                    }
                }
            }
        }
    }
}

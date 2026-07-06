using LogicWorld.Rendering.Components;
using System;
using System.IO;

using CheeseUtilMod.Shared.CustomData;
using LICC;

namespace CheeseUtilMod.Client
{
    public class Ram16bClientBase : ComponentClientCode<IRamData>, FileLoadable
    {
        private string requestingSavePath = null;

        public int addressLines;
        public ushort[] memory;
        private static int PEG_L = 2;

        protected override void Initialize()
        {
            addressLines = CodeInfoInts[0];
            memory = new ushort[1 << addressLines];
            CheeseUtilClient.fileLoadables.Add(this);
        }

        protected override void OnComponentDestroyed()
        {
            CheeseUtilClient.fileLoadables.Remove(this);
        }

        public void Load(byte[] filedata, LineWriter writer, bool force)
        {
            if (force || GetInputState(PEG_L))
            {
                var max_index = 1 << addressLines;
                if (filedata.Length / 2 < max_index)
                {
                    max_index = filedata.Length / 2;
                }
                for (int i = 0; i < max_index; i++)
                {
                    ushort lo = filedata[i * 2];
                    ushort hi = filedata[i * 2 + 1];
                    ushort val = (ushort)(lo | (hi << 8));
                    memory[i] = val;
                }
                SendDataToServer();
            }
        }

        protected void SendDataToServer()
        {
            Logger.Info("Sending data to server");
            byte[] mem1 = new byte[memory.Length * 2];
            Buffer.BlockCopy(memory, 0, mem1, 0, mem1.Length);
            Data.ClientIncomingData = Utils.Compress(mem1);
            Data.State = 1;
        }

        public void Save(string filePath)
        {
            requestingSavePath = filePath;
            Data.State = 2;
        }

        protected override void DataUpdate()
        {
            base.DataUpdate();
            if (Data.State != 3 || requestingSavePath == null) return;

            try
            {
                byte[] data = Utils.Decompress(Data.ClientIncomingData ?? []);
                Logger.Info($"[CheeseUtilMod] Got {data.Length} bytes of RAM contents from server");
                File.WriteAllBytes(requestingSavePath, data);
            }
            catch (Exception ex)
            {
                Logger.Error("[CheeseUtilMod] Saving RAM contents failed with exception: " + ex);
            }
            finally
            {
                Data.State = 0;
                requestingSavePath = null;
            }
        }

        protected override void SetDataDefaultValues()
        {
            Data.initialize();
        }
    }
}

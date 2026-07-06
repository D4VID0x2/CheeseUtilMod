using System;
using System.IO;
using LogicWorld.Rendering.Components;

using CheeseUtilMod.Shared.CustomData;
using LICC;

namespace CheeseUtilMod.Client
{
    public class Ram4bClientBase : ComponentClientCode<IRamData>, FileLoadable
    {
        private string requestingSavePath = null;

        public int addressLines;
        public byte[] memory;
        private static int PEG_L = 2;

        protected override void Initialize()
        {
            addressLines = CodeInfoInts[0];
            memory = new byte[(1 << addressLines) / 2];
            CheeseUtilClient.fileLoadables.Add(this);
        }

        protected override void OnComponentDestroyed()
        {
            CheeseUtilClient.fileLoadables.Remove(this);
        }

        public void Load(byte[] filedata, LineWriter writer, bool force)
        {
            if (force | GetInputState(PEG_L))
            {
                var max_index = (1 << addressLines) / 2;
                if (filedata.Length < max_index)
                {
                    max_index = filedata.Length;
                }
                for (int i = 0; i < max_index; i++)
                {
                    memory[i] = filedata[i];
                }
                SendDataToServer();
            }
        }

        protected void SendDataToServer()
        {
            Data.ClientIncomingData = Utils.Compress(memory);
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

using System;
using System.IO;
using LogicWorld.Rendering.Components;

using CheeseUtilMod.Shared.CustomData;
using LICC;

namespace CheeseUtilMod.Client
{
    public class RamResizableClient : ComponentClientCode<IRamResizableData>, FileLoadable
    {
        private static int PEG_L = 2;
        private string requestingSavePath = null;

        protected override void Initialize()
        {
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
                Data.ClientIncomingData = Utils.Compress(filedata);
                Data.State = 1;
            }
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

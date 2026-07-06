using LogicWorld.Rendering.Components;

using CheeseUtilMod.Shared.CustomData;
using LICC;

namespace CheeseUtilMod.Client
{
    public class DualPortRamResizableClient : ComponentClientCode<IRamResizableData>, FileLoadable
    {
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
            if (force || GetInputState(Pegs.DualPort.LOAD))
            {
                Data.ClientIncomingData = Utils.Compress(filedata);
                Data.State = 1;
            }
        }

        protected override void SetDataDefaultValues()
        {
            Data.initialize();
        }
    }
}

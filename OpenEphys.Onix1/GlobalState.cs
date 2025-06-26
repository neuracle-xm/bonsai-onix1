using System.Threading;
using OpenCV.Net;

namespace OpenEphys.Onix1;

public class GlobalState
{
    /// <summary>
    /// 头盒的状态，初始为采集状态
    /// </summary>
    public static HubState HubState { get; set; } = HubState.Data;
}


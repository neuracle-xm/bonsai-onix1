using System.Collections.Generic;
using static OpenEphys.Onix1.ConfigureHeadstage64NoAux;

namespace OpenEphys.Onix1;

public class GlobalState
{
    /// <summary>
    /// 头盒的状态，初始为采集状态
    /// </summary>
    public static Dictionary<HubName, HubState> HubStates { get; set; }

    /// <summary>
    /// 记录每个DeviceName对应的头盒
    /// </summary>
    public static Dictionary<string, HubName> DeviceNameToHubName { get; set; }
}


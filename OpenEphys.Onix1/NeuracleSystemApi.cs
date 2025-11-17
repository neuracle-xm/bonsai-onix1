using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;
using Bonsai.Osc;

namespace NeuracleExtension;

/// <summary>
/// 处理python和matlab发过来的参数
/// </summary>
public class NeuracleSystemApi : Sink<Message>
{
    public override IObservable<Message> Process(IObservable<Message> source)
    {
        return source.Select(message =>
        {
            var contents = message.GetContents();
            //contents中的内容是[deviceName,command,parameter...]
            //deviceName为UI上当前某个头盒或headstage的名称，'HubA','HeadstageA'之类
            //command和parameter有三种情况:
            //case 1: command = 'data' 没有parameter
            //case 2: command = 'stimulate' parameter为刺激参数，顺序按照UI上的来
            //case 3: command = 'impedance' parameter为阻抗通道
            List<object> result = new();
            foreach (var item in contents)
            {
                result.Add(item);
            }
            string deviceName = (string)result[0];
            //Console.WriteLine($"deviceName:{deviceName}");
            //判断是头盒还是Headstage
            var isHub = deviceName.StartsWith("Hub");
            //这俩用于头盒
            string hubStimulationName = "";
            string hubSwitchDeviceName = "";
            //这俩用于headstage
            string headstageDataDeviceName = "";
            string headstageStimulationName = "";
            if (isHub)
            {
                hubStimulationName = deviceName + "/ElectricalStimulator";
                hubSwitchDeviceName = deviceName + "/SwitchDevice";
                //Console.WriteLine($"hubStimulationName:{hubStimulationName},hubSwitchDeviceName:{hubSwitchDeviceName}");
            }
            else
            {
                headstageDataDeviceName = deviceName + "/NeuracleHeadstageData";
                headstageStimulationName = deviceName + "/HeadstageStimulator";
                //Console.WriteLine($"headstageDataDeviceName:{headstageDataDeviceName},headstageStimulationName:{headstageStimulationName}");
            }
            string command = (string)result[1];
            //Console.WriteLine($"command:{command}");
            switch (command)
            {
                case "data":
                    if (isHub)
                    {
                        NeuracleDataMode.DataProcedure(hubSwitchDeviceName);
                    }
                    else
                    {
                        NeuracleHeadstageDataMode.DataProcedure(headstageDataDeviceName);
                    }
                    break;
                case "stimulate":
                    var ch1StimulateParameter = new StimulateParameter(Convert.ToBoolean(result[2]), Convert.ToUInt32(result[3]), Convert.ToBoolean(result[4]),
                                                                       Convert.ToUInt32(result[5]), (int)result[6], Convert.ToUInt32(result[7]),
                                                                       Convert.ToUInt32(result[8]), (int)result[9], Convert.ToUInt32(result[10]),
                                                                       (int)result[11], Convert.ToUInt32(result[12]), Convert.ToUInt32(result[13]),
                                                                       Convert.ToUInt32(result[14]), Convert.ToUInt32(result[15]));
                    //Console.WriteLine($"ch1StimulateParameter:{ch1StimulateParameter}");
                    if (isHub)
                    {
                        var ch2StimulateParameter = new StimulateParameter(Convert.ToBoolean(result[16]), Convert.ToUInt32(result[17]), Convert.ToBoolean(result[18]),
                                                                           Convert.ToUInt32(result[19]), (int)result[20], Convert.ToUInt32(result[21]),
                                                                           Convert.ToUInt32(result[22]), (int)result[23], Convert.ToUInt32(result[24]),
                                                                           (int)result[25], Convert.ToUInt32(result[26]), Convert.ToUInt32(result[27]),
                                                                           Convert.ToUInt32(result[28]), Convert.ToUInt32(result[29]));
                        //Console.WriteLine($"ch2StimulateParameter:{ch2StimulateParameter}");
                        var ch3StimulateParameter = new StimulateParameter(Convert.ToBoolean(result[30]), Convert.ToUInt32(result[31]), Convert.ToBoolean(result[32]),
                                                                           Convert.ToUInt32(result[33]), (int)result[34], Convert.ToUInt32(result[35]),
                                                                           Convert.ToUInt32(result[36]), (int)result[37], Convert.ToUInt32(result[38]),
                                                                           (int)result[39], Convert.ToUInt32(result[40]), Convert.ToUInt32(result[41]),
                                                                           Convert.ToUInt32(result[42]), Convert.ToUInt32(result[43]));
                        //Console.WriteLine($"ch3StimulateParameter:{ch3StimulateParameter}");
                        var ch4StimulateParameter = new StimulateParameter(Convert.ToBoolean(result[44]), Convert.ToUInt32(result[45]), Convert.ToBoolean(result[46]),
                                                                           Convert.ToUInt32(result[47]), (int)result[48], Convert.ToUInt32(result[49]),
                                                                           Convert.ToUInt32(result[50]), (int)result[51], Convert.ToUInt32(result[52]),
                                                                           (int)result[53], Convert.ToUInt32(result[54]), Convert.ToUInt32(result[55]),
                                                                           Convert.ToUInt32(result[56]), Convert.ToUInt32(result[57]));
                        //Console.WriteLine($"ch4StimulateParameter:{ch4StimulateParameter}");
                        NeuracleStimulateMode.StimulateProcedure(hubStimulationName, hubSwitchDeviceName,
                                                                 ch1StimulateParameter, ch2StimulateParameter,
                                                                 ch3StimulateParameter, ch4StimulateParameter);
                    }
                    else
                    {
                        NeuracleHeadstageStimulateMode.StimulateProcedure(headstageStimulationName, ch1StimulateParameter);
                    }
                    break;
                case "impedance":
                    var impedanceChannelIndex = Convert.ToUInt32(result[2]);
                    //Console.WriteLine($"impedanceChannelIndex:{impedanceChannelIndex}");
                    if (isHub)
                    {
                        NeuracleImpedanceMode.ImpedanceProcedure(impedanceChannelIndex, hubStimulationName, hubSwitchDeviceName);
                    }
                    else
                    {
                        NeuracleHeadstageImpedanceMode.ImpedanceProcedure(headstageDataDeviceName);
                    }
                    break;
                default:
                    break;
            }
            return message;
        });

    }
}


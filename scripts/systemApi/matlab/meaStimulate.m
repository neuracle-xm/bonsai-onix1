function meaStimulate(u,deviceName,ch1StimulateParameter,ch2StimulateParameter,ch3StimulateParameter,ch4StimulateParameter)
    % 头盒下发刺激参数
    % 检验所有参数的合法性，如果有不合法会直接报错
    ch1StimulateParameter.checkParameters()
    ch1StimulateParameter.checkMeaStimulateChannelIndex()
    ch1StimulateParameter.checkAllCurrent(true)
    ch2StimulateParameter.checkParameters()
    ch2StimulateParameter.checkMeaStimulateChannelIndex()
    ch2StimulateParameter.checkAllCurrent(true)
    ch3StimulateParameter.checkParameters()
    ch3StimulateParameter.checkMeaStimulateChannelIndex()
    ch3StimulateParameter.checkAllCurrent(true)
    ch4StimulateParameter.checkParameters()
    ch4StimulateParameter.checkMeaStimulateChannelIndex()
    ch4StimulateParameter.checkAllCurrent(true)
    StimulateParameter.checkMeaMultiStimulateChannelIndex(ch1StimulateParameter,ch2StimulateParameter,ch3StimulateParameter,ch4StimulateParameter)
    [tagType1,args1] = stimulateParameterToOsc(ch1StimulateParameter);
    [tagType2,args2] = stimulateParameterToOsc(ch2StimulateParameter);
    [tagType3,args3] = stimulateParameterToOsc(ch3StimulateParameter);
    [tagType4,args4] = stimulateParameterToOsc(ch4StimulateParameter);
    tagType = char("ss" + tagType1 + tagType2 + tagType3 + tagType4);
    oscsend(u,'/', tagType, deviceName.name,'stimulate',args1{:}, args2{:},args3{:},args4{:});
end
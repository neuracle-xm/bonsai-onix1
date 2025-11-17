function headstageStimulate(u,deviceName,ch1StimulateParameter)
    % headstage下发刺激参数
    ch1StimulateParameter.checkParameters()
    ch1StimulateParameter.checkAllCurrent(false)
    [tagType1,args1] = stimulateParameterToOsc(ch1StimulateParameter);
    tagType = char("ss" + tagType1);
    oscsend(u,'/', tagType, deviceName.name,'stimulate',args1{:});
end
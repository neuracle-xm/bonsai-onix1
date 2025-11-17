function switchTime = dataStimSwitchTime(startTimeCsv,endTimeCsv)
    T = readtable(startTimeCsv);
    switchTimeStart = T{1,1};
    T = readtable(endTimeCsv);
    switchTimeEnd = T{1,1};
    % 头盒的HubClock之间的间隔时间,单位ns
    timeStep = 32;
    % 单位us
    switchTime = (switchTimeEnd - switchTimeStart) * timeStep / 1000;
end
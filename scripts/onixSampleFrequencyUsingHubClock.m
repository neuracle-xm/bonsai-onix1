function sampleFrequency = onixSampleFrequencyUsingHubClock(csvPath)
    T = readtable(csvPath);
    firstHubclock = T{1,1};
    lastHubclock = T{end,end};
    % 头盒的HubClock之间的间隔时间,单位ns
    timeStep = 32;
    duration = (lastHubclock - firstHubclock) * timeStep;
    n = numel(T);
    % 采样率，单位k
    sampleFrequency = (n - 1) / duration * 1000 * 1000;
end
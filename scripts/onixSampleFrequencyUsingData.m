function sampleFrequency = onixSampleFrequencyUsingData(path,duration)
    data = readOnixData(path);
    count = length(data(1:end));
    % duration单位s; 采样率,单位k
    sampleFrequency = count / duration / 1000;
end
function snr = onixSnr(path,channel,sinFrequency,sampleFrequency)
    totalData = readOnixData(path);
    data = totalData(channel,:);
    % 去均值
    data = data - mean(data);
    % 计算功率谱
    nfft = 65536 * 2;
    [pxx,f] = pwelch(data,[], [], nfft,sampleFrequency);
    % 频谱泄露
    fRange = 2;
    f1 = sinFrequency - fRange;
    f2 = sinFrequency + fRange;
    idx = f >= f1 & f <= f2;
    sinPsd = max(pxx(idx));
    meanPsd = mean(pxx(~idx));
    snr = 10 * log10(sinPsd / meanPsd);
end
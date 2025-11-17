function delta = polarizationVoltage(noBiasPath,positiveBiasPath,negativeBiasPath)
    % 读取无偏置方波
    noBiasData = readOnixData(noBiasPath);
    noBiasData = noBiasData(1,:);
    % 读取200mV偏置方波
    positiveBiasData = readOnixData(positiveBiasPath);
    positiveBiasData = positiveBiasData(1,:);
    % 读取-200mV偏置方波
    negativeBiasData = readOnixData(negativeBiasPath);
    negativeBiasData = negativeBiasData(1,:);
    % 去除工频
    Fs = 64000;
    [b,a] = iirnotch(50/(Fs/2),2/(Fs/2));
    % 查看频率响应
    % fvtool(b,a,'fs',Fs)
    noBiasDataFiltered = filter(b,a,noBiasData);
    positiveBiasDataFiltered = filter(b,a,positiveBiasData);
    positiveBiasDataFiltered = positiveBiasDataFiltered(length(positiveBiasDataFiltered) / 10:end);
    negativeBiasDataFiltered = filter(b,a,negativeBiasData);
    negativeBiasDataFiltered = negativeBiasDataFiltered(length(negativeBiasDataFiltered) / 10:end);
    % 找各自的峰峰值
    noBiasPeakToPeak = max(noBiasDataFiltered) - min(noBiasDataFiltered);
    % noBiasPeakToPeak = max(noBiasData) - min(noBiasData);
    positivePeakToPeak = max(positiveBiasDataFiltered) - min(positiveBiasDataFiltered);
    % positivePeakToPeak = max(positiveBiasData) - min(positiveBiasData);
    negativePeakToPeak = max(negativeBiasDataFiltered) - min(negativeBiasDataFiltered);
    % negativePeakToPeak = max(negativeBiasData) - min(negativeBiasData);
    % 计算误差百分比
    delta = abs((max(positivePeakToPeak,negativePeakToPeak) - noBiasPeakToPeak)) / noBiasPeakToPeak * 100;
end
function switchToImpedanceMode(u,deviceName,channelIndex)
    % 切换到阻抗模式
    if isa(deviceName,'MeaDeviceName')
        if channelIndex < 0 || channelIndex >= Constants.MEA_CHANNEL_NUMBER
            hint = "头盒阻抗通道channelIndex要在0~" + (Constants.MEA_CHANNEL_NUMBER - 1) + "内";
            error(hint)
        end
    end
    if isa(deviceName,'HeadstageDeviceName')
        if channelIndex < 0 || channelIndex >= Constants.HEADSTAGE_CHANNEL_NUMBER
            hint = "Headstage阻抗通道channelIndex要在0~" + (Constants.HEADSTAGE_CHANNEL_NUMBER - 1) + "内";
            error(hint)
        end
    end
    oscsend(u,'/','ssi', deviceName.name,'impedance',channelIndex);
end
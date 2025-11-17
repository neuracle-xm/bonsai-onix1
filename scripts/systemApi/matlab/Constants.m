classdef Constants
    properties (Constant)
        % 本机地址
        HOST = '127.0.0.1'
        % 端口号,和bonsai中的要一致,且不能被其他程序占用
        PORT = 50000
        % 头盒的最大最小电流,单位μA
        MEA_MAX_CURRENT = 4000
        MEA_MIN_CURRENT = -4000
        % 头盒的通道数
        MEA_CHANNEL_NUMBER = 64
        % headstage最大最小电流,单位μA
        HEADSTAGE_MAX_CURRENT = 2500
        HEADSTAGE_MIN_CURRENT = -2500
        % headstage的通道数
        HEADSTAGE_CHANNEL_NUMBER = 32
    end
end

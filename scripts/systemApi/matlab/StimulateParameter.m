classdef StimulateParameter
    properties
        % 是否使用双相波
        chBiphasic
        % 每个Burst的脉冲个数
        chBurstPulseCount
        % 是否使用这个通道,这个参数在headstage中无用
        chEnable
        % 各个Burst之间的间隔,单位μs
        chInterBurstInterval
        % 双相波的相间电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
        chInterPhaseCurrent
        % 双相波的相间脉宽,单位μs
        chInterPhaseInterval
        % 每个Burst内部的脉冲间隔,单位μs
        chInterPulseInterval
        % 双相波第一相的电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
        chPhaseOneCurrent
        % 双相波的第一相脉宽,单位μs
        chPhaseOneDuration
        % 双相波的第二相电流,单位μA,头盒范围-4000~4000,headstage范围-2500~2500
        chPhaseTwoCurrent
        % 双相波的第二相脉宽,单位μs
        chPhaseTwoDuration
        % 第几个通道作为刺激通道,头盒范围0~63,这个参数在headstage中无用
        chStimulateChannel
        % 每个Train的Burst的个数
        chTrainBurstCount
        % 刺激开始前的延迟,单位μs
        chTriggerDelay
    end

    methods
        function obj = StimulateParameter(chBiphasic,chBurstPulseCount,chEnable,chInterBurstInterval,chInterPhaseCurrent,...
                                          chInterPhaseInterval,chInterPulseInterval,chPhaseOneCurrent,chPhaseOneDuration,...
                                          chPhaseTwoCurrent,chPhaseTwoDuration,chStimulateChannel,chTrainBurstCount,...
                                          chTriggerDelay)
            obj.chBiphasic = chBiphasic;
            obj.chBurstPulseCount = chBurstPulseCount;
            obj.chEnable = chEnable;
            obj.chInterBurstInterval = chInterBurstInterval;
            obj.chInterPhaseCurrent = chInterPhaseCurrent;
            obj.chInterPhaseInterval = chInterPhaseInterval;
            obj.chInterPulseInterval = chInterPulseInterval;
            obj.chPhaseOneCurrent = chPhaseOneCurrent;
            obj.chPhaseOneDuration = chPhaseOneDuration;
            obj.chPhaseTwoCurrent = chPhaseTwoCurrent;
            obj.chPhaseTwoDuration = chPhaseTwoDuration;
            obj.chStimulateChannel = chStimulateChannel;
            obj.chTrainBurstCount = chTrainBurstCount;
            obj.chTriggerDelay = chTriggerDelay;
        end
        
        function checkParameters(obj)
            % 检验头盒和headstage通用的刺激参数合法性,不合法直接报错
            if obj.chBurstPulseCount <= 0
                error("chBurstPulseCount必须大于0")
            end
            if obj.chInterBurstInterval < 0
                error("chInterBurstInterval不能小于0")
            end
            if obj.chInterPhaseInterval < 0
                error("chInterPhaseInterval不能小于0")
            end
            if obj.chInterPulseInterval < 0
                error("chInterPulseInterval不能小于0")
            end
            if obj.chPhaseOneDuration < 0
                error("chPhaseOneDuration不能小于0")
            end
            if obj.chPhaseTwoDuration < 0
                error("chPhaseTwoDuration不能小于0")
            end
            if obj.chTrainBurstCount < 0
                error("chTrainBurstCount不能小于0")
            end
            if obj.chTriggerDelay < 0
                error("chTriggerDelay不能小于0")
            end
        end

        function checkMeaStimulateChannelIndex(obj)
            % 检验头盒刺激通道index是否合法
            if obj.chStimulateChannel < 0 || obj.chStimulateChannel >= Constants.MEA_CHANNEL_NUMBER
                hint = "头盒的ch_stimulate_channel必须在0~" + (Constants.MEA_CHANNEL_NUMBER - 1) + "之间";
                error(hint)
            end
        end

        function checkAllCurrent(obj,isMea)
            % 检验所有的电流属性
            StimulateParameter.checkCurrentHelper(obj.chPhaseOneCurrent, 'chPhaseOneCurrent', isMea)
            % 如果使用了双相波,再check相间和第二相
            if obj.chBiphasic
                StimulateParameter.checkCurrentHelper(obj.chInterPhaseCurrent, 'chInterPhaseCurrent', isMea)
                StimulateParameter.checkCurrentHelper(obj.chPhaseTwoCurrent, 'chPhaseTwoCurrent', isMea)
            end
        end
    end

    methods (Static)
        function checkCurrentHelper(current,currentName,isMea)
            % 检验头盒和headstage刺激电流是否合法的辅助函数,减少重复
            if isMea
                maxCurrent = Constants.MEA_MAX_CURRENT;
            else
                maxCurrent = Constants.HEADSTAGE_MAX_CURRENT;
            end
            if isMea
                minCurrent = Constants.MEA_MIN_CURRENT;
            else
                minCurrent = Constants.HEADSTAGE_MIN_CURRENT;
            end
            if isMea
                deviceName = '头盒';
            else
                deviceName = 'headstage';
            end
            if current < minCurrent || current > maxCurrent
                hint = deviceName + "的" + currentName + "必须在" + minCurrent + "~" + maxCurrent + "之间";
                error(hint)
            end
        end
        
        function obj = defaultStimulateParameter()
            % 使用默认参数构造刺激参数
            obj = StimulateParameter(false, 1, false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0);
        end

        function checkMeaMultiStimulateChannelIndex(ch1StimulateParameter,ch2StimulateParameter,ch3StimulateParameter,ch4StimulateParameter)
            % 检验头盒四个刺激通道的stimulate_parameter参数是否合法,如果用了这些通道,就不能有相同的
            chStimulateChannel1 = ch1StimulateParameter.chStimulateChannel;
            chStimulateChannel2 = ch2StimulateParameter.chStimulateChannel;
            chStimulateChannel3 = ch3StimulateParameter.chStimulateChannel;
            chStimulateChannel4 = ch4StimulateParameter.chStimulateChannel;
            stimulateChannels = [];
            if ch1StimulateParameter.chEnable
                stimulateChannels = [stimulateChannels,chStimulateChannel1];
            end
            if ch2StimulateParameter.chEnable
                stimulateChannels = [stimulateChannels,chStimulateChannel2];
            end
            if ch3StimulateParameter.chEnable
                stimulateChannels = [stimulateChannels,chStimulateChannel3];
            end
            if ch4StimulateParameter.chEnable
                stimulateChannels = [stimulateChannels,chStimulateChannel4];
            end
            uniqueChannels = unique(stimulateChannels);
            if length(stimulateChannels) ~= length(uniqueChannels)
                error('头盒四个刺激通道中的stimulateChannel不能相同')
            end
        end
    end
end

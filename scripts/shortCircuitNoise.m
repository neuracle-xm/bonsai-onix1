function shortCircuitNoise(noisePath)
    %% 输入数据
    %-----测试人员参数区-----%
    DeviceType='EEG';               % 待测试设备的类型，脑电（EEG）或肌电（EMG），测试已知
    WinLength=60;                   % 滑窗窗宽，单位秒，本测试中，固定为10秒，不用改
    WinStep=10;                     % 滑窗步进，单位秒，本测试中，固定为10秒，不用改
    WinLength_RMS=0.5;              % 计算RMS的窗宽，单位秒 
    WinStep_RMS=0.1;               % 计算RMS的步进，单位秒
    %-----采用eeglab插件导入待测试的BDF文件-----%
    %  读取BDF
    % EEG=pop_importNeuracle();
    % folder_base ='C:\Users\50609\Documents\MATLAB\W3_2k短路噪声\';
    % folder_name = '1';
    % [EEG, ~] = pop_importNeuracle({'data.bdf'}, [folder_base folder_name]);
    % Chan4Test='TP8';                % 外短路的导联名称，测试已知
    % [~,ChanLocation]=ismember(Chan4Test,{EEG.chanlocs(:).labels});
    % EEGData=double(EEG.data);
    % EEGData=EEGData(ChanLocation,:);
    % Fs=EEG.srate;
    %  读取mat
    %  读取mat文件
    % read_Intan_RHD2000_file
    % filename = 'data';
    % filename = './IRN_32P.mat';
    Fs = 64000;
    % EEG = pop_importdata('data',filename,'dataformat','array','srate',Fs);
    % 把50,100,150Hz滤掉
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 45, 'hicutoff', 55, 'revfilt', 1);
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 95, 'hicutoff', 105, 'revfilt', 1);
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 145, 'hicutoff', 155, 'revfilt', 1);
    % % 滤除7450和7550
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 7445, 'hicutoff', 7455, 'revfilt', 1);
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 7545, 'hicutoff', 7555, 'revfilt', 1);
    % % 把7500Hz滤掉
    % EEG = pop_eegfiltnew(EEG, 'locutoff', 7495, 'hicutoff', 7505, 'revfilt', 1);
    % % 把1000Hz的倍频滤掉
    % for i = 1 : 14
    %     EEG = pop_eegfiltnew(EEG, 'locutoff', i * 1000 - 5, 'hicutoff', i * 1000 + 5, 'revfilt', 1);
    % end
    % total_data =double(EEG.data);
    channel = 1;
    total_data = readOnixData(noisePath);
    EEGData = total_data(channel,:);
    % 
    if strcmp(DeviceType,'EEG')
        LowFreq=0.1;                % 高通滤波的截止频率，单位Hz，用于EEG
        HighFreq_1=40;              % 低通滤波1的截止频率，单位Hz，用于EEG
        HighFreq_2=70;              % 低通滤波2的截止频率，单位Hz，用于EEG
    %     采样率大于20000时这里修改下限截止频率和上限截止频率
        if Fs>20*1e+3
            LowFreq_1=0.1;
%             HighFreq_3=10*1e+3;   
            HighFreq_3=Fs/2 - 5;
        end
    elseif strcmp(DeviceType,'EMG')
        LowFreq=20;                 % 高通滤波的截止频率，单位Hz，用于EMG
        HighFreq_1=Fs/5;            % 低通滤波1的截止频率，单位Hz，用于EMG
        HighFreq_2=Fs/4;            % 低通滤波2的截止频率，单位Hz，用于EMG
        if Fs>20*1e+3
            LowFreq_1=300;
            HighFreq_3=10*1e+3;
        end
    end
    %% 对噪声进行两次滤波（EEG：0.5-40Hz，0.5-70Hz；EMG：20-250Hz，20-500Hz）
    % Rs=60;
    % Rp=1;
    if strcmp(DeviceType,'EEG')
        N_hp=2;
    elseif strcmp(DeviceType,'EMG')
        N_hp=4;
    end
    N_lp=3;
    WinSize=40*1e-3;
    [b_hp,a_hp]=butter(N_hp,LowFreq/(Fs/2),'high');
    [b_lp_1,a_lp_1]=butter(N_lp,HighFreq_1/(Fs/2),'low');
    [b_lp_2,a_lp_2]=butter(N_lp,HighFreq_2/(Fs/2),'low');
    [b,a]=iirnotch(50/(Fs/2),2/(Fs/2));
    
    [EEGData_Filt,Zi]=my_filter(b_hp,a_hp,EEGData,WinSize*Fs);
    [EEGData_1,Zi_1]=my_filter(b_lp_1,a_lp_1,EEGData_Filt,WinSize*Fs);
    [EEGData_2,Zi_2]=my_filter(b_lp_2,a_lp_2,EEGData_Filt,WinSize*Fs);
    [EEGData_2,Zi_3]=my_filter(b,a,EEGData_2,WinSize*Fs);
    
    if Fs>20*1e+3
        [b_hp_1,a_hp_1]=butter(N_hp,LowFreq_1/(Fs/2),'high');
        [b_lp_3,a_lp_3]=butter(N_lp,HighFreq_3/(Fs/2),'low');
        [EEGData_Filt_1,Zi_sub]=my_filter(b_hp_1,a_hp_1,EEGData,WinSize*Fs);
        [EEGData_3,Zi_4]=my_filter(b_lp_3,a_lp_3,EEGData_Filt_1,WinSize*Fs);
    end
    %% 滑窗计算噪声数据的功率谱密度、RMS及峰峰值
    WinNum=length(1:WinStep*Fs:length(EEGData)-WinLength*Fs+1);
    for win=1:WinNum
        Stock=EEGData((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
        Window=hann(length(Stock)).';
    %     Window=hamming(length(Stock)).';
    
    %     X=Stock.*Window*1.633;
    %     nFFT=length(Stock);
           nFFT=2^15;
    
    %     PSD_X=abs(fft(X,nfft)).^2/length(X)/Fs;
    %     PSD_X=abs(fft(X,1024));
    %     f=(0:length(PSD_X)-1)*(Fs/length(PSD_X));
        [PSD_X,f]=pwelch(Stock,Window,[],nFFT,Fs);
        
    %     Stock_1=EEGData((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
    %     Stock_2=EEGData((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
    %     
        Stock_1=EEGData_1((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
        Stock_2=EEGData_2((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
    
        if Fs>20*1e+3
            Stock_3=EEGData_3((win-1)*WinStep*Fs+1:(win-1)*WinStep*Fs+WinLength*Fs);
        end
    %     Vpp_1=max(Stock_1)-min(Stock_1);
    %     Vpp_2=max(Stock_2)-min(Stock_2);
        WinNum_RMS=length(1:WinStep_RMS*Fs:length(Stock_1)-WinLength_RMS*Fs+1);
        RMS_1=zeros(1,WinNum_RMS);
        RMS_2=zeros(1,WinNum_RMS);
        RMS_3=zeros(1,WinNum_RMS);
        Vpp_1=zeros(1,WinNum_RMS);
        Vpp_2=zeros(1,WinNum_RMS);
        Vpp_3=zeros(1,WinNum_RMS);
        for ii=1:WinNum_RMS
            Stock_4=Stock_1(int32((ii-1)*WinStep_RMS*Fs+1:(ii-1)*WinStep_RMS*Fs+WinLength_RMS*Fs));
            RMS_1(ii)=sqrt(mean(Stock_4.^2));
            Vpp_1(ii)=max(Stock_4)-min(Stock_4);
            Stock_5=Stock_2(int32((ii-1)*WinStep_RMS*Fs+1:(ii-1)*WinStep_RMS*Fs+WinLength_RMS*Fs));
            RMS_2(ii)=sqrt(mean(Stock_5.^2));
            Vpp_2(ii)=max(Stock_5)-min(Stock_5);
            if Fs>20*1e+3
                Stock_6=Stock_3(int32((ii-1)*WinStep_RMS*Fs+1:(ii-1)*WinStep_RMS*Fs+WinLength_RMS*Fs));
                RMS_3(ii)=sqrt(mean(Stock_6.^2));
                Vpp_3(ii)=max(Stock_6)-min(Stock_6);
            end
        end
        t_RMS=(0:length(RMS_1)-1)*WinStep_RMS+WinLength_RMS;
        
    
        figure;
        subplot(411);
        plot(f,db(PSD_X,'power'),'-b','LineWidth',1.0);
        grid on;
        xlabel('频率（Hz）');
        ylabel('功率谱密度（dB）');
        if strcmp(DeviceType,'EEG')
    %         这里可以改画图时的频率最大值
            xlim([0 Fs/2]);
        elseif strcmp(DeviceType,'EMG')
            xlim([0,Fs/4]);
        end
        title(strcat('第',num2str(win),'段数据：噪声PSD（无滤波）'));
    
        subplot(412);
        plot(t_RMS,RMS_1,'-k','LineWidth',1.0);
        grid on;
        hold on;
        plot(t_RMS,Vpp_1,'-r','LineWidth',1.0);
        xlabel('时间（秒）');
        ylabel('幅值（μV）');
        title(strcat('第',num2str(win),'段数据（',num2str(LowFreq),'-',num2str(HighFreq_1),'Hz）：有效值（',num2str(mean(RMS_1),3),'±',num2str(std(RMS_1),3),'μV），峰峰值（',num2str(mean(Vpp_1),3),'±',num2str(std(Vpp_1),3),'μV）'));
        xlim([0 max(t_RMS)]);
        legend('有效值','峰峰值','Location','SouthWest');
    
        subplot(413);
        plot(t_RMS,RMS_2,'-k','LineWidth',1.0);
        grid on;
        hold on;
        plot(t_RMS,Vpp_2,'-r','LineWidth',1.0);
        xlabel('时间（秒）');
        ylabel('幅值（μV）');
        title(strcat('第',num2str(win),'段数据（',num2str(LowFreq),'-',num2str(HighFreq_2),'Hz）：有效值（',num2str(mean(RMS_2),3),'±',num2str(std(RMS_2),3),'μV），峰峰值（',num2str(mean(Vpp_2),3),'±',num2str(std(Vpp_2),3),'μV）'));
        xlim([0 max(t_RMS)]);
        legend('有效值','峰峰值','Location','SouthWest');
    
        subplot(414);
        plot(t_RMS,RMS_3,'-k','LineWidth',1.0);
        grid on;
        hold on;
        plot(t_RMS,Vpp_3,'-r','LineWidth',1.0);
        xlabel('时间（秒）');
        ylabel('幅值（μV）');
        title(strcat('第',num2str(win),'段数据（',num2str(LowFreq_1),'-',num2str(HighFreq_3),'Hz）：有效值（',num2str(mean(RMS_3),3),'±',num2str(std(RMS_3),3),'μV），峰峰值（',num2str(mean(Vpp_3),3),'±',num2str(std(Vpp_3),3),'μV）'));
        xlim([0 max(t_RMS)]);
        legend('有效值','峰峰值','Location','SouthWest');
    
        PLI_Thd=30;    % 工频阈值，单位dB
        if strcmp(DeviceType,'EEG')
    %         if db(PSD_X(round(50/Fs*length(PSD_X))))-mean(db(PSD_X(round(50/Fs*length(PSD_X))-9:round(50/Fs*length(PSD_X))+10)))>=PLI_Thd-10
    %             warndlg(strcat('第',num2str(win),'段数据存在工频干扰！'),'数据异常提示');
    %         end
        elseif strcmp(DeviceType,'EMG')
            if db(PSD_X(round(50/Fs*length(PSD_X))))-mean(db(PSD_X(round(50/Fs*length(PSD_X))-9:round(50/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(100/Fs*length(PSD_X))))-mean(db(PSD_X(round(100/Fs*length(PSD_X))-9:round(100/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(150/Fs*length(PSD_X))))-mean(db(PSD_X(round(150/Fs*length(PSD_X))-9:round(150/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(200/Fs*length(PSD_X))))-mean(db(PSD_X(round(200/Fs*length(PSD_X))-9:round(200/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(250/Fs*length(PSD_X))))-mean(db(PSD_X(round(250/Fs*length(PSD_X))-9:round(250/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(300/Fs*length(PSD_X))))-mean(db(PSD_X(round(300/Fs*length(PSD_X))-9:round(300/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(350/Fs*length(PSD_X))))-mean(db(PSD_X(round(350/Fs*length(PSD_X))-9:round(350/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(400/Fs*length(PSD_X))))-mean(db(PSD_X(round(400/Fs*length(PSD_X))-9:round(400/Fs*length(PSD_X))+10)))>=PLI_Thd||...
               db(PSD_X(round(450/Fs*length(PSD_X))))-mean(db(PSD_X(round(450/Fs*length(PSD_X))-9:round(450/Fs*length(PSD_X))+10)))>=PLI_Thd
                warndlg(strcat('第',num2str(win),'段数据存在工频干扰！'),'数据异常提示');
            end
        end
    end
end
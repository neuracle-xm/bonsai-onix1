function cmrr(path,InputAmp)
    N_FIR=512;            % FIR滤波器的阶数，建议为512
    BW_FIR=1;             % FIR滤波器的通带宽度，建议为1Hz
    InputFreq=10;         % 输入正弦波的频率，单位Hz，测试者已知,10或60
%     InputAmp=2000/2;      % 共模输入值（峰峰值为2V，幅度为1V），单位mV，测试者已知
    ChanSelect=1;         % 测试数据所在的通道，测试者已知
    LineFreq=50;
    data = readOnixData(path);
    data = data(ChanSelect,:);
    Fs = 64000;
    EEGData = data;
    % EEGData = data/1000;    % eeglab的单位从μV转为mV
    ChanNum=size(EEGData,1);
    t=(0:size(EEGData,2)-1)*(1/Fs);
    InputData=InputAmp*sin(2*pi*InputFreq*t);
    DataLength=size(EEGData,2)/Fs;
    OutputData=EEGData(ChanSelect,:);
    %% 时域滤波法与LMS法
    %-----FIR滤波-----%
    Wn=[InputFreq-BW_FIR/2,InputFreq+BW_FIR/2]/(Fs/2);
    BPFilter=fir1(N_FIR,Wn,'bandpass',tukeywin(N_FIR+1));
    % figure;
    % freqz(BPFilter,1,10*Fs,Fs);
    OutputData_Filt=filtfilt(BPFilter,1,OutputData);
    TestTimeRange = 1:length(OutputData_Filt);
    % if DataLength>10
    %     TestTimeRange=round(DataLength/2*Fs)-5*Fs+1:round(DataLength/2*Fs)+6*Fs;
    %     OutputData_Filt=OutputData_Filt(TestTimeRange);
    % end
    DotNum=length(OutputData_Filt);
    t=(0:DotNum-1)*(1/Fs);
    % %-----LMS自适应逼近10Hz正弦-----%
    % x1=sin(2*pi*InputFreq*t);
    % x2=cos(2*pi*InputFreq*t);
    % 
    % w1=0.1;
    % w2=0.1;
    % 
    % OutputData_LMS=zeros(1,DotNum);
    % Error=zeros(1,DotNum);
    % mu=0.01;
    % 
    % for ii=1:DotNum
    %     OutputData_LMS(ii)=w1*x1(ii)+w2*x2(ii);
    %     Error(ii)=OutputData(ii)-OutputData_LMS(ii);
    %     w1=w1+mu*Error(ii)*x1(ii);
    %     w2=w2+mu*Error(ii)*x2(ii);
    % end
    %-----滑窗计算峰峰值的误差及其绝对值-----%
    if InputFreq==1
        WindowLength=2*Fs;
    else
        WindowLength=1*Fs;
    end
    % WindowLength = 5*Fs;
    VppLength=ceil(2*(1/InputFreq)*Fs);
    VppNum=floor(WindowLength/VppLength);
    VppValue=zeros(1,VppNum);
    Step=1*Fs;
    Step(Step>=WindowLength)=WindowLength;
    WindowNum=length(1:Step:DotNum)-ceil(WindowLength/Step);
    % Error_Vpp=zeros(1,WindowNum);
    Vpp=zeros(1,WindowNum);
    CMRR=zeros(1,WindowNum);
    
    for win=1:WindowNum
        Stock=OutputData_Filt((win-1)*Step+1:(win-1)*Step+WindowLength);
        for ii=1:VppNum
            Stock_1=Stock(VppLength*(ii-1)+1:VppLength*ii);
            [x,y]=sort(Stock_1,'descend');
            if y(1)>(1/InputFreq)*Fs*0.6
                VppValue(1,ii)=x(1)-min(Stock_1(y(1)-floor((1/InputFreq)*Fs*0.6):y(1)));
            else
                VppValue(1,ii)=x(1)-min(Stock_1(y(1):y(1)+floor((1/InputFreq)*Fs*0.6)));
            end
            Vpp(1,win)=mean(VppValue);
    %         Error_Vpp(1,win)=2*InputAmp-mean(VppValue);
            CMRR(1,win)=20*log10(2*InputAmp/mean(VppValue));
        end
    end
    t_Vpp=(0:WindowNum-1)*Step/Fs;
    %% 绘制结果
    % t_signal = 60;
    % figure;
    % subplot(311);
    % plot(t,InputData(TestTimeRange),'-b','LineWidth',1.5);
    % xlabel('时间（秒钟）');
    % ylabel('信号幅度（mV）');
    % title('输入正弦波（理论值）');
    % xlim([0 t_signal]);
    % ylim([-InputAmp-5 InputAmp+5]);
    % grid on;
    % 
    % subplot(312);
    % plot(t,OutputData(TestTimeRange),'-r','LineWidth',1.5);
    % xlabel('时间（秒钟）');
    % ylabel('信号幅度（mV）');
    % title('输出信号（含工频和白噪声）');
    % xlim([0 t_signal]);
    % grid on;
    % 
    % subplot(313);
    % plot(t,OutputData_Filt,'-k','LineWidth',1.5);
    % xlabel('时间（秒钟）');
    % ylabel('信号幅度（mV）');
    % title('输出正弦波（FIR滤波后）');
    % xlim([0 t_signal]);
    % % ylim([-InputAmp-5 InputAmp+5]);
    % grid on;
    
    figure;
    % subplot(211);
    % plot(1:WindowNum,Vpp,'-bs','LineWidth',1.5);
    % xlim([1 WindowNum]);
    % xlabel(strcat('测试窗索引（窗宽=',num2str(WindowLength/Fs),'秒）'));
    % ylabel('输出峰峰值（mV）');
    % title(strcat('均值：',num2str(mean(Vpp(1:end)),6),'mV，标准差：',num2str(std(Vpp(1:end)),3),'mV'));
    % grid on;
    
    % subplot(111);
    plot(1:WindowNum,CMRR,'-r*','LineWidth',1.5);
    xlim([1 WindowNum]);
    xlabel(strcat('测试窗索引（窗宽=',num2str(WindowLength/Fs),'秒）'));
    ylabel('共模抑制比（dB）');
    s = strcat('共模抑制比均值：',num2str(mean(CMRR(1:end)),6),'dB，标准差：',num2str(std(CMRR(1:end)),3),'dB');
    title(s,'FontSize',14);
    grid on;
    
    % figure;
    % plot(t,OutputData_LMS);
    % xlim([0 20]); 
end
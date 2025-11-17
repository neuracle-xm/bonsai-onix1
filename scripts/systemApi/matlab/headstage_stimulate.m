disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
ch1StimulateParameter = StimulateParameter.defaultStimulateParameter();
ch1StimulateParameter.chBiphasic = true;
ch1StimulateParameter.chEnable = true;
ch1StimulateParameter.chStimulateChannel = 0;
ch1StimulateParameter.chPhaseOneCurrent = 1000;
ch1StimulateParameter.chPhaseTwoCurrent = -500;
headstageStimulate(u, HeadstageDeviceName.HeadstageA, ch1StimulateParameter);
fclose(u);
disp('end')
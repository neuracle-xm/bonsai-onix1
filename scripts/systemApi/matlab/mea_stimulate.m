disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
ch1StimulateParameter = StimulateParameter.defaultStimulateParameter();
ch1StimulateParameter.chBiphasic = true;
ch1StimulateParameter.chEnable = true;
ch1StimulateParameter.chStimulateChannel = 0;
ch1StimulateParameter.chPhaseOneCurrent = 1000;
ch1StimulateParameter.chPhaseTwoCurrent = -500;
ch2StimulateParameter = StimulateParameter.defaultStimulateParameter();
ch2StimulateParameter.chEnable = true;
ch2StimulateParameter.chStimulateChannel = 1;
ch3StimulateParameter = StimulateParameter.defaultStimulateParameter();
ch3StimulateParameter.chEnable = false;
ch3StimulateParameter.chStimulateChannel = 2;
ch4StimulateParameter = StimulateParameter.defaultStimulateParameter();
ch4StimulateParameter.chEnable = true;
ch4StimulateParameter.chStimulateChannel = 3;
meaStimulate(u,MeaDeviceName.HubA, ch1StimulateParameter,ch2StimulateParameter,ch3StimulateParameter,ch4StimulateParameter);
fclose(u);
disp('end')
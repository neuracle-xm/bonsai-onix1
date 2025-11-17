disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
switchToDataMode(u, MeaDeviceName.HubA);
fclose(u);
disp('end')
disp('start')
u = udp(Constants.HOST,Constants.PORT);
fopen(u);
switchToImpedanceMode(u, MeaDeviceName.HubA, 0);
fclose(u);
disp('end')
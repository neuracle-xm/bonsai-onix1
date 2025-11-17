from pythonosc.udp_client import UDPClient

from utils import (
    HOST,
    PORT,
    MeaDeviceName,
    StimulateParameter,
    mea_stimulate,
)

if __name__ == "__main__":
    print("Start")
    client = UDPClient(HOST, PORT)
    ch1_stimulate_parameter = StimulateParameter.default_stimulate_parameter()
    ch1_stimulate_parameter.ch_bi_phasic = True
    ch1_stimulate_parameter.ch_enable = True
    ch1_stimulate_parameter.ch_stimulate_channel = 0
    ch1_stimulate_parameter.ch_phase_one_current = 1000
    ch1_stimulate_parameter.ch_phase_two_current = -500
    ch2_stimulate_parameter = StimulateParameter.default_stimulate_parameter()
    ch2_stimulate_parameter.ch_enable = False
    ch2_stimulate_parameter.ch_stimulate_channel = 1
    ch3_stimulate_parameter = StimulateParameter.default_stimulate_parameter()
    ch3_stimulate_parameter.ch_enable = True
    ch3_stimulate_parameter.ch_stimulate_channel = 2
    ch4_stimulate_parameter = StimulateParameter.default_stimulate_parameter()
    ch4_stimulate_parameter.ch_enable = True
    ch4_stimulate_parameter.ch_stimulate_channel = 3
    mea_stimulate(
        client,
        MeaDeviceName.HubA,
        ch1_stimulate_parameter,
        ch2_stimulate_parameter,
        ch3_stimulate_parameter,
        ch4_stimulate_parameter,
    )
    print("End")

from pythonosc.udp_client import UDPClient

from utils import (
    HOST,
    PORT,
    StimulateParameter,
    headstage_stimulate,
    HeadstageDeviceName,
)

if __name__ == "__main__":
    print("Start")
    client = UDPClient(HOST, PORT)
    ch1_stimulate_parameter = StimulateParameter.default_stimulate_parameter()
    ch1_stimulate_parameter.ch_phase_one_current = 1000
    ch1_stimulate_parameter.ch_phase_two_current = -500
    headstage_stimulate(client, HeadstageDeviceName.HeadstageA, ch1_stimulate_parameter)
    print("End")

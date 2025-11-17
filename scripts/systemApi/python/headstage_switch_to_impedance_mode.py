from pythonosc.udp_client import UDPClient

from utils import (
    HOST,
    PORT,
    HeadstageDeviceName,
    switch_to_impedance_mode,
)

if __name__ == "__main__":
    print("Start")
    client = UDPClient(HOST, PORT)
    switch_to_impedance_mode(client, HeadstageDeviceName.HeadstageA, 0)
    print("End")

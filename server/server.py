#-*- coding:utf-8 -*-
import sys
import os
import socketserver
import socket

class ServerHandler(socketserver.StreamRequestHandler):
    """ ServerHandler """

    def handle(self):

        data = b""
        while True:
            data += self.request.recv(4096)
            if len(data) == 0:
                print("Close受信")
                break

            bytelist = list(data)
            if bytelist[-3:] == [0xff, 0xff, 0xff]:
                with open("./screen.jpg", "wb") as img_f:
                    img_f.write(data)
                    data = b""

        self.request.close()

def main():
    """ main """
    socketserver.ThreadingTCPServer.allow_reuse_address = True
    server = socketserver.ThreadingTCPServer((socket.gethostname(), 20000), ServerHandler)
    server.serve_forever()

if __name__ == "__main__":
    main()

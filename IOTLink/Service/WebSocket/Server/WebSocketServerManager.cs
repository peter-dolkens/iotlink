﻿using IOTLink.Platform.WebSocket;
using IOTLinkAPI.Helpers;
using IOTLinkAPI.Platform.WebSocket;
using IOTLinkService.Engine.MQTT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace IOTLink.Service.WSServer
{
    internal class WebSocketServerManager : WebSocketBehavior
    {
        public const string WEBSOCKET_URI = "ws://localhost:9799";
        private static WebSocketServerManager _instance;

        private WebSocketServer _server;

        private Dictionary<string, string> _clients = new Dictionary<string, string>();

        public static WebSocketServerManager GetInstance()
        {
            if (_instance == null)
                _instance = new WebSocketServerManager();

            return _instance;
        }

        private WebSocketServerManager()
        {

        }

        internal void Init()
        {
            if (_server != null)
                Disconnect();

            _server = new WebSocketServer(IPAddress.Loopback, 9799);
            _server.AddWebSocketService("/", () => this);
            _server.Start();
        }

        internal void Disconnect()
        {
            if (_server == null)
                return;

            if (_server.IsListening)
                _server.Stop();

            _server = null;
        }

        internal bool IsConnected()
        {
            return _server != null && _server.IsListening;
        }

        protected override async Task OnMessage(MessageEventArgs e)
        {
            if (e.Data == null || e.Data.Length == 0)
                return;

            if (e.Opcode != Opcode.Text)
                return;

            string data = e.Text.ReadToEnd();
            if (string.IsNullOrWhiteSpace(data))
                return;

            try
            {
                dynamic json = JsonConvert.DeserializeObject<dynamic>(data);
                MessageType messageType = json.messageType;
                switch (messageType)
                {
                    case MessageType.CLIENT_REQUEST:
                        ParseClientRequest(json.content);
                        break;

                    case MessageType.CLIENT_RESPONSE:
                        ParseClientResponse(json.content);
                        break;

                    case MessageType.API_MESSAGE:
                        ParseAPIMessage(json.content);
                        break;

                    default:
                        LoggerHelper.Warn("WebSocket client send an invalid message type ({0}) with data: {1}", json.messageType, json.data);
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        internal void ParseClientRequest(dynamic content)
        {
            RequestTypeClient type = content.type;
            dynamic data = content.data;

            LoggerHelper.Debug("ParseClientRequest - Request Type: {0} | Data: {1}", type, data);
            switch (type)
            {
                case RequestTypeClient.REQUEST_CONNECTED:
                    ParseClientConnected(data);
                    break;

                case RequestTypeClient.REQUEST_PUBLISH_MESSAGE:
                    ParsePublishMessage(data);
                    break;

                default:
                    LoggerHelper.Warn("ParseClientRequest - Invalid Request Type: {0}", type);
                    break;
            }
        }

        internal void ParseClientConnected(dynamic data)
        {
            if (!string.IsNullOrWhiteSpace(data.username))
            {
                string username = ((string)data.username).Trim().ToLowerInvariant();
                _clients[username] = Id;
            }
        }

        internal void ParsePublishMessage(dynamic data)
        {
            string topic = data.topic;
            byte[] payload = data.payload;

            if (string.IsNullOrWhiteSpace(topic) || payload == null || payload.Length == 0)
                return;

            MQTTClient.GetInstance().PublishMessage(topic, payload);
        }

        internal void ParseClientResponse(dynamic content)
        {
            ResponseTypeClient type = content.type;
            dynamic data = content.data;

            LoggerHelper.Debug("ParseClientRequest - Request Type: {0} | Data: {1}", type, data);
            switch (type)
            {
                case ResponseTypeClient.RESPONSE_ADDON:
                    ParseAddonResponse(data);
                    break;

                default:
                    LoggerHelper.Warn("ParseClientRequest - Invalid Request Type: {0}", type);
                    break;
            }
        }

        private void ParseAddonResponse(dynamic data)
        {
            string addonId = data.addonId;
            dynamic addonData = data.addonData;

            if (string.IsNullOrWhiteSpace(addonId))
                return;

            LoggerHelper.Trace("ParseAddonRequest - AddonId: {0} AddonData: {1}", addonId, addonData);
        }

        internal void ParseAPIMessage(dynamic content)
        {
            LoggerHelper.Debug("ParseAPIMessage: {0}", content);
        }

        internal void SendRequest(RequestTypeServer type, dynamic data = null, string username = null)
        {
            if (!IsConnected())
                return;

            if (string.IsNullOrWhiteSpace(username))
                username = null;
            else
                username = username.Trim().ToLowerInvariant();

            dynamic msg = new ExpandoObject();
            msg.messageType = MessageType.SERVER_REQUEST;
            msg.content = new ExpandoObject();
            msg.content.type = type;
            msg.content.data = data;

            string payload = JsonConvert.SerializeObject(msg);

            if (username == null)
                Sessions.Broadcast(payload);
            else if (_clients.ContainsKey(username))
                Sessions.SendTo(_clients[username], payload);
            else
                LoggerHelper.Error("WebSocketServer - Agent from {0} not found.", username);
        }
    }
}

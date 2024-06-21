# Azure WCF Service with ROT13 Encoding

This project implements a Windows Communication Foundation (WCF) service on Azure, featuring ROT13 encoding for blob storage. It consists of two main components:

1. A WCF service that allows encoding and retrieving text.
2. A worker role that processes encoding requests.

## Features

- Encode text using ROT13 algorithm
- Store encoded text in Azure Blob Storage
- Retrieve encoded text from Azure Blob Storage
- Use Azure Queue Storage for communication between service and worker role

## Usage

The service exposes two main methods:

1. `Koduj(string nazwa, string tresc)`: Encodes the given text and stores it in a blob.
2. `Pobierz(string nazwa)`: Retrieves the encoded text from a blob.

The worker role continuously processes encoding requests from the queue.

# Multiplayer Lobby System

## 📌 Overview

This project is a **multiplayer lobby management system** built with Unity, using **Unity Netcode for GameObjects, Unity Relay**, and **Unity Lobby Services**. It enables players to **authenticate, create lobbies, join lobbies by code, or quick join** existing lobbies seamlessly.

The workflow follows Unity’s recommended **Relay + Lobby** structure for peer-to-peer multiplayer games, providing a scalable and secure connection layer.


---

## 🚀 Features

- **Authentication**
 - Anonymous sign-in via Unity Authentication Service
 - Player name assignment and storage in `PlayerPrefs`

- **Lobby System**
 - Create lobbies with custom names
 - Set maximum player count dynamically
 - Option to lock or make lobbies private
 - Retrieve and copy join codes
 - Player join/leave tracking in real-time

- **Relay Integration**
 - Automatic relay allocation for host and clients
 - Secure **DTLS** (Datagram TLS) connections for reliable data transfer
 - WebSocket Secure **(WSS)** support for WebGL builds

- **UI System**
 - Authentication menu
 - Lobby creation/join menu
 - Active lobby display with player cards

---

## 🛠️ Tech Stack
- **Unity** (2021+ recommended)
- **Unity Netcode for GameObjects**
- **Unity Relay Service**
- **Unity Lobby Service**
- **TextMeshPro** for UI input and display
- **C# 8.0+**

---

## 📂 Project Structure
```
/Scripts
 ├── Multiplayer.cs                # Core singleton manager for auth, lobby, and relay
 ├── AuthenticationMenu.cs         # Handles sign-in UI and transition to lobby menu
 ├── CreateOrJoinLobbyMenu.cs      # UI logic for creating/joining lobbies
 ├── LobbyMenu.cs                  # Displays active lobby state and player list
 └── PlayerInLobbyCard.cs          # UI representation of each player in the lobby
```

---

## ⚙️ Setup Instructions

### 1️⃣ Unity Services Setup

1. Open the **Unity Dashboard.**
2. Enable the following services:
 - Authentication
 - Relay
 - Lobby

3. Link your Unity project to the Unity Dashboard (via **Project Settings → Services**).

---

### 2️⃣ Install Required Packages

Use Unity Package Manager and install:
- com.unity.netcode.gameobjects
- com.unity.transport
- com.unity.services.relay
- com.unity.services.lobby
- com.unity.services.authentication

---

### 3️⃣ Configure Script Execution
- Attach the Multiplayer script to a persistent GameObject in your initial scene.
- Ensure DontDestroyOnLoad is enabled for that GameObject.

---

### 4️⃣ Scene Flow
1. **Authentication Scene**
Player enters a nickname → signs in → proceeds to lobby menu.

2. **Lobby Menu**
 - Create a lobby
 - Join by code
 - Quick join

3. **Active Lobby Scene**
Displays players in the lobby and manages relay connection setup.

---

## 🖼️ UI Flow

Step	Description	UI Screenshot

|1	|Authentication screen where the player inputs their nickname.|
|2	|Lobby menu where the player can create, join, or quick join a lobby.	|
|3	|Active lobby screen displaying lobby info and connected players.	|



---

## 🔄 Core Workflow

The multiplayer flow follows this state diagram:
```
Authenticate
  ↓
Initialize → Sign In
  ↓
 ┌──────────────┬──────────────┐
 │   Host       │   Client     │
 │              │              │
Create Lobby    Join Lobby
Allocate Relay  Quick Join Lobby
Get Join Code   Get Join Code
Create Lobby    Get Relay Allocation
Update Lobby    Update Network Manager
Update Network Manager with Relay Data
  ↓                     ↓
Start Host          Start Client
```

---

## 🧩 Key Classes

**`Multiplayer.cs`**

Handles:
- Authentication (`CreateUser`)
- Lobby creation (`CreateLobby`)
- Lobby joining (`JoinLobby, QuickJoinLobby`)
- Relay allocation and transport setup


**`AuthenticationMenu.cs`**
- UI controller for nickname input and sign-in.

**`CreateOrJoinLobbyMenu.cs`**
- Manages lobby creation, joining by code, and quick join actions.

**`LobbyMenu.cs`**
- Displays lobby information and manages the dynamic player list.

**`PlayerInLobbyCard.cs`**
- UI component representing individual players in the lobby.

---

## 🧪 Testing
- Run the project in **two editor instances** or **build a standalone player** to test multiplayer interactions.
- Use Unity's **Network Manager HUD** (optional) for debugging.

---

## ⚠️ Known Limitations
- No full error handling for connection drops.
- No ready/unready status for players (extendable).
- Currently uses anonymous sign-in (extendable to full account systems).

---

## 📌 Future Improvements
- Add matchmaking with filters.
- Integrate player profiles with persistent stats.
- Implement a ready system and game state synchronization.
- Support for dedicated server hosting.



---

## 📜 License

This project is released under the **MIT License**. You are free to use, modify, and distribute this project as per the license terms.


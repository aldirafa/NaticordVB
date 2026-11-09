# Naticord
Naticord is a Discord client focused on bringing the best experience possible natively.

Naticord uses DirectUI and Windows Forms, both bringing a native look to the client. DirectUI is an undocumented language made by Microsoft and was never released publicly. The library used in Naticord for DirectUI is [DirectUI.Net](https://github.com/n1d3v/DirectUI.Net).

> **About this fork:** NaticordVB is a fork of [Naticord/naticord](https://github.com/Naticord/naticord) with the whole codebase rewritten in **VB.NET** and moved from .NET Framework 4.8 to **.NET 10**. The look is the same as upstream; the chat features are being built here (see the [Roadmap](#roadmap)).

## Building
Requirements: Windows, the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```powershell
dotnet build NaticordVB/NaticordVB.vbproj -c Release
```

The output ends up in `NaticordVB/bin/Release/net10.0-windows/`. The `User Interface` and `ExtResources` folders are copied next to the executable automatically.

**One extra step: the native DirectUI bridge.** `DirectUI.Net` needs a native `dui_bridge.dll`, which is **not included in this repository**. Build the `DUI-Bridge` project from [n1d3v/DirectUI.Net](https://github.com/n1d3v/DirectUI.Net) (needs Visual Studio's *Desktop development with C++* workload) and place the result in:

```
NaticordVB/Libraries/x64/dui_bridge.dll
```

Anything inside `NaticordVB/Libraries/` is copied to the output folder on build. Without the DLL the app stops at startup with `dui_bridge.dll not found`.


<p align="center">
  <a href="https://www.star-history.com/#aldirafa/NaticordVB&Date">
   <picture>
     <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=aldirafa/NaticordVB&type=Date&theme=dark" />
     <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=aldirafa/NaticordVB&type=Date" />
     <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=aldirafa/NaticordVB&type=Date" />
   </picture>
  </a>
</p>

# Roadmap
✔️ done · ➖ partly done · 🔨 planned · 🤔 undecided · ❌ won't happen

What changed, release by release, lives in the [CHANGELOG](CHANGELOG.md).

## ✔️ What is finished
- **Signing in**
  - With a token: ✔️
  - With a QR code: ✔️
  - With e-mail and password: ❌ ([See why...](#why-cant-you-login-in-using-an-e-mail-and-password))
- **Chatting**
  - Direct messages and group DMs, newest activity first: ✔️
  - Servers, with their text and announcement channels grouped by category: ✔️
  - Reading: the latest 50 messages, "Load earlier messages", and new messages every few seconds while the app is in front: ✔️
  - Messages show avatars, names and times, group messages from the same person, split days, show replies, and list attachments, link previews and stickers as links: ✔️
  - Mentions, channels, custom emoji and `<t:...>` times are turned into readable text: ✔️
  - Click a message to pick it, then `Ctrl+C` (or right-click, *Copy text*) copies what the author wrote: ✔️
  - Sending, `Enter` to send and `Shift+Enter` for a new line, replying to a specific message, and a separate unsent draft for every chat: ✔️
  - Unread marks (bold title and a dot): ➖ they are tracked on this PC only, so something you read in another app still shows as unread here
- **Your account**
  - Status: online, idle, do not disturb, invisible: ✔️
  - Custom status with a Unicode emoji and an optional "clear after": ✔️
- **Looks**
  - Text in every script your PC has a font for, Sundanese included: ✔️
  - No crash when you are offline: the app says so and keeps what it has: ✔️
  - User interface: ➖ still work in progress

## 🔨 What we are working on next
1. **Gateway** (Discord's live connection). New, edited and deleted messages without waiting for the next refresh, typing, presence, and your real read state so unread marks agree with your other devices.
2. **Markdown** (bold, italic, underline, strikethrough, code, quotes, spoilers, headers, lists, links) and selecting *part* of a message, not only the whole message.
3. **Non-text channels** shown in the server list (voice, stage, forum). Opening them is a different question, see below.
4. **Search box** in the sidebar (it is only a placeholder now), and an emoji picker, including server emoji for the custom status.

## 🤔 Still undecided
- **How to draw text for markdown.** Keep the current Windows GDI text, or move to DirectWrite (through a NuGet package). DirectWrite brings colour emoji, automatic font fallback (also while typing), selection and custom fonts, but it is a bigger change.
- **How to read markdown.** A ready-made parser (for example Markdig, plus Discord's own syntax) or a small one of our own.
- **Voice and video**, and stage channels.
- **Threads and forum channels.**
- **Pictures:** previews and a viewer for images, and uploading files.
- **Reactions**, and editing or deleting your own messages.
- **Notifications** (toasts, tray icon), a friends list and a member list.
- **Showing typed text in scripts like Sundanese.** Today they appear as empty boxes in the box you type in (the sent message looks right), because the standard Windows text box has no font fallback.

## ⚠️ Known limits
- Edits and deletions made by other people are not shown until you reopen the chat.
- The first time a chat is seen it counts as read.
- Names are display names or usernames; server nicknames and role colours are not loaded. (yet)
- Emoji are drawn with plain Windows text: partly monochrome, and server emoji are not images.
- Using a user token goes against Discord's terms of service. Use it at your own risk.

## Supported systems (Windows only)
| System | Status |
|---|---|
| Windows 11 and Windows 10 | ✔️ |
| Windows 8.1 and Windows 7 | ❌ not supported by .NET 10. The original Naticord, built on .NET Framework 4.8, is the one to use there. |
| Windows Vista and Windows XP | ❌ |

# Why can't you login in using an e-mail and password?
Discord uses intense security measures to prevent custom clients from signing in using an e-mail and password, meaning there is no reliable way to sign in with this method without getting banned on a custom client. This is why Naticord is moving away from this approach and using easier methods instead like using a QR code to sign in.

This feature will not be coming in the future as it is a pain to maintain and I am not willing to fight through the pain of just getting to sign in.
# Credits
NaticordVB would have not been possible without these wonderful people!
- [Skymu](https://skymu.app) - Discord library used in Naticord (Also made by me!)
- [UnderscoreAngel](https://underscoreangel.neocities.org/) - Creating the current Naticord logo!

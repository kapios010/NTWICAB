# Now That's What I Call A Broadcast

> A program for (ultimately) viewing and streaming broadcasts over the Internet that work kinda like analog TV.

## 🚀 Key features
- [ ] Viewing online streams
- [x] Viewing local files as streams
- [ ] Broadcasting streams
- [x] Quitting the app

## 📡 Stream features
- Variable width, height (up to 255x255 px) and framerate (0.25 to 63.25 fps)
- Video name and author display
- [Network bug](https://en.wikipedia.org/wiki/Digital_on-screen_graphic) displayed separately from video
- 4096 available colors

## 📺 Okay but you say analog tv, what does that mean?
Well, excluding the metadata the stream is structured in such a way, so that it doesn't change individual pixel colors, but rather
modifies the color of the "cursor" that is then written to the "screen". You can't change the colors directly. 

For example, the cursor for the last pixel was `#FFF` and you want it to be `#AAA`, then you send a subtract command with value `#555`.

Also the beam scans progressively, so from left to right in each row, top to bottom. Revolutionary, right?

## 📄 File Structure

### Header (offset 0)

8 bytes of `NTWICAB ` - `0x4E, 0x54, 0x57, 0x49, 0x43, 0x41, 0x42, 0x00`

### Metadata (offset 8)

- `n` = 1 byte for amount of bytes in name
- `n` byte(s) for name in Unicode UTF-16 LE
- `m` = 1 byte for amount of bytes in author
- `m` byte(s) for author in Unicode UTF-16 LE
- 1 byte of network bug flag ( `0x80 = 0b10000000` - network bug included, anything else (preferably `0x00`) - bug not included )
  - If flag is true, next 128 bytes are Unicode characters making up the network bug (4 rows of 16 chars)
- 1 byte of width
- 1 byte of height
- 1 byte of framerate in frames per 4 seconds

### Stream Data

After the header, we pair every two bytes into a segment. The first 4 bits of the segment determine its function, and the rest provides essentially the function arguments.

In functions marked with 🎨, the rest of the bits acts as the values of RGB, each 4 bits (example: cursor is `#000000`, command is `1ABC`, new cursor will be `#AABBCC`).

First byte | Function   | Explanation | Rest of the bits
-----------|------------|-------------|-------------------
`0x0`      | Hold       | Moves cursor forward without changing its color |
`0x1`      | Add (OperatorPPP) | Adds to the cursor | 🎨
`0x2`      | Subtract (OperatorMMM) | Subtracts from the cursor | 🎨
`0x3`      | OperatorPPM | Adds RG, subtracts B | 🎨
`0x4`      | OperatorPMP | Adds RB, subtracts G | 🎨
`0x5`      | OperatorMPP | Adds GB, subtracts R | 🎨
`0x6`      | OperatorMMP | Subtracts RG, adds B | 🎨
`0x7`      | OperatorMPM | Subtracts RB, adds G | 🎨
`0x8`      | OperatorPMM | Subtracts GB, adds R | 🎨
`0x9`      | Reserved
`0xA`      | BcstEndFrame | Recalls cursor to (0,0), starts wait if correct framerate value isn't reached
`0xB`      | BcstUpdateMeta | Updates stream metadata. The bytes following this command should follow the same structure as in [Metadata](#metadata-offset-8)
`0xC`      | Reserved
`0xD`      | Reserved
`0xE`      | Reserved
`0xF`      | BcstEnd  | Ends stream


## ✨ AI Usage
None.


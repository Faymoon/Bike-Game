# Bike-Game
Little VR bike game with Godot Engine

## Build
Follow this [tutorial](https://docs.godotengine.org/en/stable/getting_started/workflow/export/android_custom_build.html#doc-android-custom-build) of the godot engine documentation.

## VR setup
If you have a cardboard_like VR headset this game should work for it, in order to have optimal experience consider modifying the `MobileVRInterface`'s parameters in [src/Game.cs](https://github.com/Faymoon/Bike-Game/blob/master/src/Game.cs#L54).

### For cardboard compatible headset
You have to scan your device's QR Code with a small cardboard logo in. You'll get a link with base 64 encoded data after the `p=`, to decode this data you need protoc, that you can download [here](https://github.com/protocolbuffers/protobuf/releases) (download just the protoc-<version>-*.zip if you don't need to compile it yourself). 
You'll also need the protocol description file, for google cardboard data you can find it [here](https://github.com/google/wwgc/blob/master/www/CardboardDevice.proto).

When you have protoc and this file, here are the steps to follow :
```bash
$ echo <insert your base 64 encoded data here> > encoded
```
This data should look something like this : `ChJULlQuIEludGVybmF0aW9uYWwSEzNEIFZSIFZpZXdlciBEZWx1eGUdJQaBPSUK1yM9KhAAAEhCAABIQgAASEIAAEhCWAE1KVwPPToICtcjPArXIzxQAWAB`
```
$ base64 -d encoded > data
```
This command convert the base 64 data to raw data.
```
$ protoc --decode=DeviceParams CardboardDevice.proto < data
```
This command decode the raw data to give you the wanted values and print them.

then you can change the data in the code (don't forget that units can be differents).

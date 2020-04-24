# Bike-Game
Little VR bike game with Godot Engine

## Build
Follow this [tutorial](https://docs.godotengine.org/en/stable/getting_started/workflow/export/android_custom_build.html#doc-android-custom-build) of the godot engine documentation.

## VR setup
If you have a cardboard-like VR headset this game should work for it, in order to have an optimal experience consider modifying the `MobileVRInterface`'s parameters in [src/Game.cs](https://github.com/Faymoon/Bike-Game/blob/master/src/Game.cs#L54).

### For cardboard compatible headset
 1. Scan your device's QR Code with a small cardboard logo in. You'll get a link with base 64 encoded data after the `p=`. 
 2. To decode this data you need [protoc](https://github.com/protocolbuffers/protobuf/releases) (download just the protoc-\<version\>-*.zip if you don't need to compile it yourself). 

3. You'll also need the protocol description file, for google cardboard data you can find it [here](https://github.com/google/wwgc/blob/master/www/CardboardDevice.proto).

4. `$ echo <insert your base 64 encoded data here> > encoded`
This data should look something like this : `ChJULlQuIEludGVybmF0aW9uYWwSEzNEIFZSIFZpZXdlciBEZWx1eGUdJQaBPSUK1yM9KhAAAEhCAABIQgAASEIAAEhCWAE1KVwPPToICtcjPArXIzxQAWAB`
5. `$ base64 -d encoded > data` This command converts the base 64 data to raw data.
6. `protoc --decode=DeviceParams CardboardDevice.proto < data` This command decodes the raw data to give you the wanted values and prints them into the appropriate file.

Then you can change the data in the code (don't forget that units can be differents).

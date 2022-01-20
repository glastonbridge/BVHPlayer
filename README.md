# A BVH Player for Unity

This is some hacky code I wrote for animating BVH files exported from [MocapNET](https://github.com/FORTH-ModelBasedTracker/MocapNET), combined with MakeHuman models that use the [CMU+Face](http://www.makehumancommunity.org/content/cmu_plus_face.html) rig. It's a work in progress, it's not expected to work out of the box. But it's fairly simple and I'm sure you can work out how to extend it to other uses, you smart cookie you.

# Installation and use

1. Copy the `Scripts` folder to your Unity project `Assets` folder
2. Import your MakeHuman model to Unity and add it to the scene
3. Open up the makehuman model in the scene hierarchy, and find the `CMU+Face compliant skeleton` sub-object
4. Attach a `Skeleton Animator` script to the skeleton
5. Import a bvh file to your assets folder, and add its path in to the `Bvh Name` property

If your BVH file plays back too quickly (say, it runs at 30fps and your game runs at 60fps), you can set a divider value to divide the frame time.

# License (MIT)

Copyright 2022 Glastonbridge Software Limited

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

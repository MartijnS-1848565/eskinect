using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    //this class is used to return the detection of a gesture together with the index of the skeleton on which it was detected.
    internal class SkeletonGesture
    {
        private bool isGesture;
        private int skeletonIndex;

        public SkeletonGesture(bool isGesture, int skeletonIndex) { 
            this.isGesture = isGesture;
            this.skeletonIndex = skeletonIndex;
        }

        public bool hasGesture() {
            return isGesture;
        }
        public int getSkeletonIndex() {
            return skeletonIndex;
        }
    }
}

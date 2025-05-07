// <copyright file="XRFaceParameterIndices.cs" company="Google LLC">
//
// Copyright 2024 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.Extensions
{
    /// <summary>
    /// This is an enumeration of the blendshape values in the Google default vector format.
    /// <see href="go/gnome-reduced-expressions-internal">
    /// </summary>
    public enum XRFaceParameterIndices
    {
        /// <summary> Left eyebrow lowerer control parameter. </summary>
        BrowLowererL = 0,

        /// <summary> Right eyebrow lowerer control parameter. </summary>
        BrowLowererR = 1,

        /// <summary> Left cheek puff control parameter. </summary>
        CheekPuffL = 2,

        /// <summary> Right cheek puff control parameter. </summary>
        CheekPuffR = 3,

        /// <summary> Left cheek raiser control parameter. </summary>
        CheekRaiserL = 4,

        /// <summary> Right cheek raiser control parameter. </summary>
        CheekRaiserR = 5,

        /// <summary> Left cheek suck control parameter. </summary>
        CheekSuckL = 6,

        /// <summary> Right cheek puff control parameter. </summary>
        CheekSuckR = 7,

        /// <summary> Bottom chin raiser control parameter. </summary>
        ChinRaiserB = 8,

        /// <summary> Top chin raiser control parameter. </summary>
        ChinRaiserT = 9,

        /// <summary> Left dimpler control parameter. </summary>
        DimplerL = 10,

        /// <summary> Right dimpler control parameter. </summary>
        DimplerR = 11,

        /// <summary> Left eye closed control parameter. </summary>
        EyesClosedL = 12,

        /// <summary> Right eye closed control parameter. </summary>
        EyesClosedR = 13,

        /// <summary> Left eye look down control parameter. </summary>
        EyesLookDownL = 14,

        /// <summary> Right eye look down control parameter. </summary>
        EyesLookDownR = 15,

        /// <summary> Left eye look left control parameter. </summary>
        EyesLookLeftL = 16,

        /// <summary> Right eye look left control parameter. </summary>
        EyesLookLeftR = 17,

        /// <summary> Left eye look right control parameter. </summary>
        EyesLookRightL = 18,

        /// <summary> Right eye look right control parameter. </summary>
        EyesLookRightR = 19,

        /// <summary> Left eye look up control parameter. </summary>
        EyesLookUpL = 20,

        /// <summary> Right eye look up control parameter. </summary>
        EyesLookUpR = 21,

        /// <summary> Inner left eyebrow raiser control parameter. </summary>
        InnerBrowRaiserL = 22,

        /// <summary> Inner right eyebrow raiser control parameter. </summary>
        InnerBrowRaiserR = 23,

        /// <summary> Jaw drop control parameter. </summary>
        JawDrop = 24,

        /// <summary> Jaw moved left control parameter. </summary>
        JawSidewaysLeft = 25,

        /// <summary> Jaw moved right control parameter. </summary>
        JawSidewaysRight = 26,

        /// <summary> Jaw thrust forward control parameter. </summary>
        JawThrust = 27,

        /// <summary> Left lid tightener control parameter. </summary>
        LidTightenerL = 28,

        /// <summary> Right lid tightener control parameter. </summary>
        LidTightenerR = 29,

        /// <summary> Lip left corner depressor control parameter. </summary>
        LipCornerDepressorL = 30,

        /// <summary> Lip right corner depressor control parameter. </summary>
        LipCornerDepressorR = 31,

        /// <summary> Lip left corner puller control parameter. </summary>
        LipCornerPullerL = 32,

        /// <summary> Lip right corner puller control parameter. </summary>
        LipCornerPullerR = 33,

        /// <summary> Left bottom lip funnler control parameter. </summary>
        LipFunnelerLB = 34,

        /// <summary> Left top lip funnler control parameter. </summary>
        LipFunnelerLT = 35,

        /// <summary> Right bottom lip funnler control parameter. </summary>
        LipFunnelerRB = 36,

        /// <summary> Right top lip funnler control parameter. </summary>
        LipFunnelerRT = 37,

        /// <summary> Left lip presser control parameter. </summary>
        LipPressorL = 38,

        /// <summary> Left lip presser control parameter. </summary>
        LipPressorR = 39,

        /// <summary> Left lip pucker control parameter. </summary>
        LipPuckerL = 40,

        /// <summary> Right lip pucker control parameter. </summary>
        LipPuckerR = 41,

        /// <summary> Left lip stretch control parameter. </summary>
        LipStretcherL = 42,

        /// <summary> Right lip stretch control parameter. </summary>
        LipStretcherR = 43,

        /// <summary> Bottom left lip suck control parameter. </summary>
        LipSuckLB = 44,

        /// <summary> Left top suck control parameter. </summary>
        LipSuckLT = 45,

        /// <summary> Bottom right lip suck control parameter. </summary>
        LipSuckRB = 46,

        /// <summary> Right top suck control parameter. </summary>
        LipSuckRT = 47,

        /// <summary> Left lip tightener control parameter. </summary>
        LipTightenerL = 48,

        /// <summary> Right lip tightener control parameter. </summary>
        LipTightenerR = 49,

        /// <summary> Lips moved together control parameter. </summary>
        LipsToward = 50,

        /// <summary> Lower left lip depresser control parameter. </summary>
        LowerLipDepressorL = 51,

        /// <summary> Lower right lip depresser control parameter. </summary>
        LowerLipDepressorR = 52,

        /// <summary> Mouth left motion control parameter. </summary>
        MouthLeft = 53,

        /// <summary> Mouth right motion control parameter. </summary>
        MouthRight = 54,

        /// <summary> Left nose wrinkle control parameter. </summary>
        NoseWrinklerL = 55,

        /// <summary> Right nose wrinkle control parameter. </summary>
        NoseWrinklerR = 56,

        /// <summary> Outer left eyebrow raiser control parameter. </summary>
        OuterBrowRaiserL = 57,

        /// <summary> Outer right eyebrow raiser control parameter. </summary>
        OuterBrowRaiserR = 58,

        /// <summary> Left upper lid raiser control parameter. </summary>
        UpperLidRaiserL = 59,

        /// <summary> Right upper lid raiser control parameter. </summary>
        UpperLidRaiserR = 60,

        /// <summary> Upper left lip depresser control parameter. </summary>
        UpperLipRaiserL = 61,

        /// <summary> Upper left lip depresser control parameter. </summary>
        UpperLipRaiserR = 62,

        /// <summary> Upper left lip depresser control parameter. </summary>
        TongueOut = 63,

        /// <summary> Upper left lip depresser control parameter. </summary>
        TongueLeft = 64,

        /// <summary> Upper left lip depresser control parameter. </summary>
        TongueRight = 65,

        /// <summary> Upper left lip depresser control parameter. </summary>
        TongueUp = 66,

        /// <summary> Upper left lip depresser control parameter. </summary>
        TongueDown = 66
    }
}

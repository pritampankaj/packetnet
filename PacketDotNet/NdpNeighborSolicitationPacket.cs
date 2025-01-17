﻿/*
This file is part of PacketDotNet.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PacketDotNet.Ndp;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    public class NdpNeighborSolicitationPacket : NdpPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NdpNeighborSolicitationPacket" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        public NdpNeighborSolicitationPacket(ByteArraySegment header) : base(header)
        { }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public List<NdpOption> OptionsCollection
        {
            get => ParseOptions(OptionsSegment);
            set
            {
                var optionsOffset = NdpFields.NeighborSolicitationOptionsAddressOffset;

                foreach (var option in value)
                {
                    var optionBytes = option.Bytes;
                    Array.Copy(optionBytes, 0, Header.Bytes, Header.Offset + optionsOffset, optionBytes.Length);
                    optionsOffset += optionBytes.Length;
                }
            }
        }

        /// <summary>
        /// Gets the options as a <see cref="ByteArraySegment" />.
        /// </summary>
        public ByteArraySegment OptionsSegment
        {
            get
            {
                var optionsOffset = NdpFields.NeighborSolicitationOptionsAddressOffset;
                var optionsLength = Header.Length - optionsOffset;

                return new ByteArraySegment(Header.Bytes, Header.Offset + optionsOffset, optionsLength);
            }
        }

        /// <summary>
        /// Gets or sets the target address.
        /// </summary>
        public IPAddress TargetAddress
        {
            get => IPPacket.GetIPAddress(AddressFamily.InterNetworkV6, Header.Offset + NdpFields.NeighborSolicitationTargetAddressOffset, Header.Bytes);
            set
            {
                var address = value.GetAddressBytes();

                for (var i = 0; i < address.Length; i++)
                    Header.Bytes[Header.Offset + NdpFields.NeighborSolicitationTargetAddressOffset + i] = address[i];
            }
        }

        /// <inheritdoc />
        public override string ToString(StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            var color = "";
            var colorEscape = "";

            if (outputFormat is StringOutputType.Colored or StringOutputType.VerboseColored)
            {
                color = Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }

            switch (outputFormat)
            {
                case StringOutputType.Normal:
                case StringOutputType.Colored:
                    // build the output string
                    buffer.AppendFormat("{0}[NdpNeighborSolicitationPacket: TargetAddress={2}]{1}",
                                        color,
                                        colorEscape,
                                        TargetAddress);

                    break;

                case StringOutputType.Verbose:
                case StringOutputType.VerboseColored:
                    // collect the properties and their value
                    var properties = new Dictionary<string, string>
                    {
                        { "targetAddress", TargetAddress.ToString() }
                    };

                    // calculate the padding needed to right-justify the property names
                    var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                    // build the output string
                    buffer.AppendLine("NDP:  ******* NDP - \"Neighbor Solicitation\"- offset=? length=" + TotalPacketLength);
                    buffer.AppendLine("NDP:");
                    foreach (var property in properties)
                    {
                        buffer.AppendLine("NDP: " + property.Key.PadLeft(padLength) + " = " + property.Value);
                    }

                    buffer.AppendLine("NDP:");
                    break;
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
    }
}
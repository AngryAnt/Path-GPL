/*
    Path - pathfinding system for the Unity engine

    Copyright (C) 2008 Emil E. Johansen

    This file is part of Path.

    Path is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 2 of the License, or
    (at your option) any later version.

    Path is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Path.  If not, see <http://www.gnu.org/licenses/>.

    For alternative license options, contact the copyright holder.

    Emil E. Johansen emil@eej.dk
*/


using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PathLibrary
{

	[ Serializable() ]	
	public class ConnectionAsset : TaggedAsset, IComparable
	{
		private NetworkNodeAsset from, to;
		private float width;
		private bool enabled;
		private float weightFactor;



		public ConnectionAsset( NetworkNodeAsset from, NetworkNodeAsset to, float width, CollectionAsset collection ) : base( collection )
		{
			this.from = from;
			this.to = to;
			this.width = width;
			this.enabled = true;
			this.weightFactor = 1.0f;
		}



		public NetworkNodeAsset From
		{
			get
			{
				return from;
			}
			set
			{
				from = value;
			}
		}



		public NetworkNodeAsset To
		{
			get
			{
				return to;
			}
			set
			{
				to = value;
			}
		}



		public float Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}



		public float WeightFactor
		{
			get
			{
				return weightFactor;
			}
			set
			{
				weightFactor = value;
			}
		}



		public float Cost
		{
			get
			{
				return ( From.Position - To.Position ).magnitude * weightFactor;
			}
		}



		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if( enabled != value )
				{
					enabled = value;

					if( Control.Instance != null && !enabled )
					{
						Control.Instance.OnDisabled( this );
					}
				}
			}
		}



		public override string ToString()
		{
			return to.ToString();
		}



		public int CompareTo( object other )
		{
			ConnectionAsset otherConnection;

			otherConnection = other as ConnectionAsset;

			if( otherConnection != null )
			{
				return Cost.CompareTo( otherConnection.Cost );
			}
			else
			{
				throw new ArgumentException( "Cannot compare to non-connection object" );
			}
		}
	}

}
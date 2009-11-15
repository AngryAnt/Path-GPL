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

namespace PathLibrary {
	
	[ Serializable() ]
	public abstract class NetworkAsset : TaggedAsset
	{
		private string name;
		private float x, y, z, sX, sY, sZ;
		private bool enabled;
		
		
		
		public NetworkAsset( string name, CollectionAsset collection ) : base( collection )
		{
			this.name = name;
			x = y = z = 0;
			sX = sY = sZ = 10.0f;
			enabled = true;
		}
		
		
		
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
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
			return Name;
		}
		
		
		
		public abstract NetworkNodeAsset[] Nodes
		{
			get;
		}
		
		
		
		public NetworkNodeAsset GetNode( string name )
		{
			foreach( NetworkNodeAsset node in Nodes )
			{
				if( node.Name == name )
				{
					return node;
				}
			}
			
			return null;
		}
		
		
		
		public ArrayList GetNodesByTag( string tag )
		{
			ArrayList found;
			
			found = new ArrayList();
			
			foreach( NetworkNodeAsset node in Nodes )
			{
				if( node.HasTag( tag ) )
				{
					found.Add( node );
				}
			}
			
			return found;
		}
				
		
		
		public Vector3 Position
		{
			get
			{
				return new Vector3( x, y, z );
			}
			set
			{
				x = value.x;
				y = value.y;
				z = value.z;
			}
		}
		
		
		
		public Vector3 Size
		{
			get
			{
				return new Vector3( sX, sY, sZ );
			}
			set
			{
				sX = value.x;
				sY = value.y;
				sZ = value.z;
			}
		}
		
		
		
		public virtual void OnRenderGizmos( Transform transform, float standardMarkerSize )
		{
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork == this )
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube( transform.position + Position, Size );
			}
		}
	}
	
}
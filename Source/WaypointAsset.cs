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
	public class WaypointAsset : NetworkNodeAsset
	{
		private float radius;
		private bool disableRuntime;

		
		
		public WaypointAsset( string name, Vector3 position, float radius, NetworkAsset network, CollectionAsset collection ) : base( network, collection )
		{
			this.Name = name;
			this.Position = position;
			this.radius = radius;
			disableRuntime = true;
		}
		
		
		
		public WaypointAsset( WaypointAsset original, NetworkAsset network ) : base( network, original.Collection )
		{
			this.Name = original.Name;
			this.Position = original.Position;
			this.radius = original.Radius;
			this.disableRuntime = original.DisableRuntime;
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			// Copy the tag indexes
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
		}
		
		
				
		public bool DisableRuntime
		{
			get
			{
				return disableRuntime;
			}
			set
			{
				disableRuntime = value;
			}
		}
		
		
		
		public float Radius
		{
			get
			{
				return radius;
			}
			set
			{
				radius = value;
			}
		}



		public override bool ContainsPoint( Vector3 point, Transform transform )
		{
			Vector3 absolutePosition;
			
			absolutePosition = GetAbsolutePosition( transform );
			
			return ( point - absolutePosition ).magnitude <= radius;
		}
		
		
		
		public override Vector3 GetNearestPoint( Vector3 point, float radius, Transform transform )
		{
			Vector3 absolutePosition;
					
			if( Radius < radius || ContainsPoint( point, transform ) )
			// Cannot contain the unit or the unit is already inside our radius
			{
				return point;
			}
			
			absolutePosition = GetAbsolutePosition( transform );
			
			return absolutePosition + ( point - absolutePosition ).normalized * ( Radius - radius );
		}



		public override void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection )
		{
			OnRenderGizmos( transform, standardMarkerSize, offset, networkSelected, nodeSelected, selectedConnection, false );
		}



		public void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection, bool onlySelectedConnection )
		{
			if( !onlySelectedConnection )
			{
				Gizmos.color = ( !networkSelected ) ? Color.yellow : ( nodeSelected ) ? Color.white : Color.green;
		    	Gizmos.DrawWireSphere( Position + transform.position + offset, radius );
			}
			
			foreach( ConnectionAsset connection in Connections )
			{
				Vector3 from, to, vector, fromOffsetA, fromOffsetB, toOffsetA, toOffsetB;
				
				if( onlySelectedConnection && selectedConnection != connection )
				{
					continue;
				}
				
				from = Position + transform.position + offset;
				to = connection.To.Position + transform.position + offset;
				vector = to - from;
				fromOffsetA = from + vector.normalized * ( connection.From as WaypointAsset ).Radius + Vector3.Cross( vector, Vector3.up ).normalized * -( connection.Width / 2.0f );
				fromOffsetB = from + vector.normalized * ( connection.From as WaypointAsset ).Radius + Vector3.Cross( vector, Vector3.up ).normalized * ( connection.Width / 2.0f );
				toOffsetA = to - vector.normalized * ( connection.To as WaypointAsset ).Radius + Vector3.Cross( vector, Vector3.up ).normalized * -( connection.Width / 2.0f );
				toOffsetB = to - vector.normalized * ( connection.To as WaypointAsset ).Radius + Vector3.Cross( vector, Vector3.up ).normalized * ( connection.Width / 2.0f );

				Gizmos.color = ( !networkSelected ) ? Color.yellow : ( selectedConnection == connection ) ? Color.white : Color.green;

				Gizmos.DrawLine( fromOffsetA, toOffsetA );
				Gizmos.DrawLine( fromOffsetB, toOffsetB );
			}
		}
	}

}
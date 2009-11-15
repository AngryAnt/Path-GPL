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
	public class GridNodeAsset : NetworkNodeAsset
	{
		private NetworkNodeAsset target;
		
		
		
		public GridNodeAsset( string name, GridNetworkAsset network, CollectionAsset collection ) : base( network, collection )
		{
			this.Name = name;
		}
		
		
		
		public GridNodeAsset( GridNodeAsset original, NetworkAsset network ) : base( network, original.Collection )
		{
			this.Name = original.Name;
			target = original.Target;
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			// Copy the tag indexes
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
		}
		
		
		
		public NetworkNodeAsset Target
		{
			get
			{
				return target;
			}
			set
			{
				if( value is GridNodeAsset )
				{
					Debug.LogError( "Cannot have a grid node be based on another grid node" );
					return;
				}
				
				target = value;
			}
		}



		public ConnectionAsset[] TargetConnections
		{
			get
			{
				return Target.Connections;
			}
		}



		public override Vector3 Position
		{
			get
			{
				return target.Position;
			}
			set
			{
				target.Position = value;
			}
		}
		
		
		
		public override Vector3 GetAbsolutePosition( Transform transform )
		{
			return target.GetAbsolutePosition( transform );
		}
		
		
		
		public override bool ContainsPoint( Vector3 point, Transform transform )
		{
			return target.ContainsPoint( point, transform );
		}
		
		
		
		public override Vector3 GetNearestPoint( Vector3 point, float radius, Transform transform )
		{
			return target.GetNearestPoint( point, radius, transform );
		}
		
		
		
		public override void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection )
		{
			OnRenderGizmos( transform, standardMarkerSize, offset, networkSelected, nodeSelected, selectedConnection, false );
		}



		public void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection, bool onlySelectedConnection )
		{
			if( target == null )
			{
				return;
			}
			
			if( networkSelected )
			{
				if( target is WaypointAsset )
				{
					( ( WaypointAsset )target ).OnRenderGizmos( transform, standardMarkerSize, offset, networkSelected, nodeSelected, selectedConnection, true );
				}
				else
				{
					target.OnRenderGizmos( transform, standardMarkerSize, offset, networkSelected, nodeSelected, selectedConnection );
				}
			}
			
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere( transform.position + offset + Position, standardMarkerSize );
			
			
			foreach( ConnectionAsset connection in Connections )
			{
				Vector3 from, to, vector, fromOffsetA, fromOffsetB, toOffsetA, toOffsetB;
				
				if( onlySelectedConnection && selectedConnection != connection )
				{
					continue;
				}
				
				from = Position + transform.position + Target.Network.Position;
				to = connection.To.Position + transform.position + ( ( GridNodeAsset )connection.To ).Target.Network.Position;
				vector = to - from;
				fromOffsetA = from + vector.normalized + Vector3.Cross( vector, Vector3.up ).normalized * -( connection.Width / 2.0f );
				fromOffsetB = from + vector.normalized + Vector3.Cross( vector, Vector3.up ).normalized * ( connection.Width / 2.0f );
				toOffsetA = to - vector.normalized + Vector3.Cross( vector, Vector3.up ).normalized * -( connection.Width / 2.0f );
				toOffsetB = to - vector.normalized + Vector3.Cross( vector, Vector3.up ).normalized * ( connection.Width / 2.0f );

				Gizmos.color = ( !networkSelected ) ? Color.yellow : ( selectedConnection == connection ) ? Color.white : Color.green;

				Gizmos.DrawLine( fromOffsetA, toOffsetA );
				Gizmos.DrawLine( fromOffsetB, toOffsetB );
			}
		}
	}
}
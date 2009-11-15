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
	
	[Serializable()]
	public class WaypointNetworkAsset : NetworkAsset
	{
		private WaypointAsset[] waypoints;
		
		
		
		public WaypointNetworkAsset( string name, CollectionAsset collection ) : base( name, collection )
		{
			waypoints = new WaypointAsset[ 0 ];
		}
		
	
		
		public WaypointNetworkAsset( WaypointNetworkAsset original ) : base( original.Name, original.Collection )
		{
			this.Position = original.Position;
			this.Size = original.Size;
			this.waypoints = new WaypointAsset[ original.Nodes.Length ];
			for( int i = 0; i < this.Waypoints.Length; i++ )
			// Create new waypoint assets - based on the nodes of the original
			{
				this.Waypoints[ i ] = new WaypointAsset( ( WaypointAsset )original.Nodes[ i ], this );
			}
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			// Copy the tag indexes
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
			
			for( int cellIDX = 0; cellIDX < this.waypoints.Length; cellIDX++ )
			// Create connections going from OUR node at the same index as THEIR node to same - just with to
			{
				this.waypoints[ cellIDX ].Connections = new ConnectionAsset[ original.Nodes[ cellIDX ].Connections.Length ];
				for( int connectionIDX = 0; connectionIDX < this.waypoints[ cellIDX ].Connections.Length; connectionIDX++ )
				{
					this.waypoints[ cellIDX ].Connections[ connectionIDX ] = new ConnectionAsset(
						this.waypoints[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].From ) ],
						this.waypoints[ Array.IndexOf( original.Nodes, original.Nodes[ cellIDX ].Connections[ connectionIDX ].To ) ],
						original.Nodes[ cellIDX ].Connections[ connectionIDX ].Width,
						this.Collection );
						
					this.waypoints[ cellIDX ].Connections[ connectionIDX ].Tags = new int[ original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length ];
					for( int tagIDX = 0; tagIDX < original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags.Length; tagIDX++ )
					// Copy the tag indexes
					{
						this.waypoints[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ] = original.Nodes[ cellIDX ].Connections[ connectionIDX ].Tags[ tagIDX ];
					}
				}
			}
		}
		
		
		
		public WaypointAsset[] Waypoints
		{
			get
			{
				return waypoints;
			}
		}
		
		
		
		public override NetworkNodeAsset[] Nodes
		{
			get
			{
				return Waypoints;
			}
		}
		
		
		
		public WaypointAsset Get( string name )
		{
			foreach( WaypointAsset asset in waypoints )
			{
				if( asset.Name == name )
				{
					return asset;
				}
			}
			
			return null;
		}
		
		
		
		public WaypointAsset Add( WaypointAsset waypoint )
		{
			WaypointAsset[] newWaypoints;
			newWaypoints = new WaypointAsset[ waypoints.Length + 1 ];
			
			for( int i = 0; i < waypoints.Length; i++ )
			{
				if( waypoints[ i ].Name == waypoint.Name )
				{
					return null;
				}
				
				newWaypoints[ i ] = waypoints[ i ];
			}
			
			newWaypoints[ waypoints.Length ] = waypoint;
			waypoints = newWaypoints;
			
			return waypoint;
		}
		
		
		
		public bool Remove( WaypointAsset waypoint )
		{
			bool found;
			WaypointAsset[] newWaypoints;
			
			found = false;
			newWaypoints = new WaypointAsset[ waypoints.Length - 1 ];
			
			for( int i = 0; i < newWaypoints.Length; i++ )
			{
				if( waypoints[ i ] == waypoint )
				{
					found = true;
				}

				newWaypoints[ i ] = waypoints[ i + ( ( found ) ? 1 : 0 ) ];
			}
			
			found = ( waypoints[ newWaypoints.Length ] == waypoint ) ? true : found;
			
			if( !found )
			{
				return false;
			}
			
			waypoints = newWaypoints;

			return true;
		}



		public void AutoConnect( Transform transform, LayerMask layerMask, float newConnectionWidth )
		{
			Vector3 direction;
			
			foreach( WaypointAsset waypoint in Waypoints )
			{
				if( !waypoint.Enabled )
				{
					continue;
				}
				
				foreach( WaypointAsset other in Waypoints )
				{
					if( !other.Enabled || waypoint == other )
					{
						continue;
					}
					
					direction = other.Position - waypoint.Position;
					if( !Physics.Raycast( waypoint.Position + Position + transform.position + direction.normalized * waypoint.Radius, direction, direction.magnitude - waypoint.Radius - other.Radius, layerMask ) &&
					 	!waypoint.ConnectsTo( other ) )
					{
						waypoint.ConnectTo( other, newConnectionWidth );
					}
				}
			}
		}



		public void Disconnect()
		{
			foreach( WaypointAsset waypoint in Waypoints )
			{
				waypoint.Disconnect();
			}
		}



		public override void OnRenderGizmos( Transform transform, float standardMarkerSize )
		{
			base.OnRenderGizmos( transform, standardMarkerSize );
			
			foreach( WaypointAsset waypoint in Nodes )
			// Render all nodes and connections
			{
				waypoint.OnRenderGizmos( transform, standardMarkerSize, Position,
					( Editor.Instance != null && Editor.Instance.SelectedNetwork == this ),
					( Editor.Instance != null && Editor.Instance.SelectedNode.Contains( waypoint ) ),
					( Editor.Instance != null ) ? Editor.Instance.SelectedConnection : null
				);
			}
			
			if( Editor.Instance != null && Editor.Instance.SelectedNetwork == this && Editor.Instance.SelectedNode.Count == 1 && Editor.Instance.SelectedConnection != null )
			// Re-render selected connection
			{
				( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] ).OnRenderGizmos( transform, standardMarkerSize, Position,
					true,
					true,
					Editor.Instance.SelectedConnection,
					true
				);
			}
		}
	}
	
	
	
	
}
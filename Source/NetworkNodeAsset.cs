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
	public class NetworkNodeAsset : TaggedAsset, IComparable
	{
		private string name;
		private float x, y, z;
		private ConnectionAsset[] connections;
		private bool enabled;
		private NetworkAsset network;
		
		
		public NetworkNodeAsset( NetworkAsset network, CollectionAsset collection ) : base( collection )
		{
			x = y = z = 0.0f;
			connections = new ConnectionAsset[ 0 ];
			enabled = true;
			this.network = network;
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
		
		
		
		public virtual Vector3 Position
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
		
		
		
		public virtual Vector3 GetAbsolutePosition( Transform transform )
		{
			return transform.TransformPoint( Position + Network.Position );
		}
		
		
		
		public Vector3 AbsolutePosition
		{
			get
			{
				if( Control.Instance == null || Control.Instance.Owner == null || Control.Instance.Owner.transform == null )
				{
					Debug.LogError( "Unable to access the current Control instance or its transform" );
					return Position;
				}
				
				return GetAbsolutePosition( Control.Instance.Owner.transform );
			}
		}
		
		
		
		public NetworkAsset Network
		{
			get
			{
				return network;
			}
		}
		
		
		
		public ConnectionAsset[] Connections
		{
			get
			{
				return connections;
			}
			set
			{
				connections = value;
			}
		}
		
		
		
		public override string ToString()
		{
			return name;
		}
		
		
		
		public void AddConnection( ConnectionAsset connection )
		{
			ConnectionAsset[] newConnections;
			
			if( ConnectsTo( connection.To as NetworkNodeAsset ) )
			{
				return;
			}
			
			newConnections = new ConnectionAsset[ connections.Length + 1 ];
			
			for( int i = 0; i < connections.Length; i++ )
			{
				newConnections[ i ] = connections[ i ];
			}
			
			newConnections[ connections.Length ] = connection;
			connections = newConnections;
		}
		
		
		
		public bool RemoveConnection( ConnectionAsset connection )
		{
			bool found;
			ConnectionAsset[] newConnections;
			
			found = false;
			newConnections = new ConnectionAsset[ connections.Length - 1 ];
			
			for( int i = 0; i < newConnections.Length; i++ )
			{
				if( connections[ i ] == connection )
				{
					found = true;
				}

				newConnections[ i ] = connections[ i + ( ( found ) ? 1 : 0 ) ];
			}
			
			found = ( connections[ newConnections.Length ] == connection ) ? true : found;
			
			if( !found )
			{
				return false;
			}
			
			connections = newConnections;

			return true;
		}
		
		
		
		public ConnectionAsset GetConnection( NetworkNodeAsset destination )
		{
			foreach( ConnectionAsset connection in connections )
			{
				if( connection.To == destination )
				{
					return connection;
				}
			}
			
			return null;
		}
		
		
		
		public ArrayList GetConnections( NetworkNodeAsset destination )
		{
			ArrayList result;
			
			result = new ArrayList();
			foreach( ConnectionAsset connection in connections )
			{
				if( connection.To == destination )
				{
					result.Add( connection );
				}
			}
			
			return result;
		}
		
		
		
		public bool ConnectsTo( NetworkNodeAsset destination )
		{
			return GetConnection( destination ) != null;
		}
		
		
		
		public bool ConnectTo( NetworkNodeAsset destination, float width )
		{
			if( ConnectsTo( destination ) )
			{
				return false;
			}
			
			AddConnection( new ConnectionAsset( this, destination, width, Collection ) );
			
			return true;
		}
		
		
		
		public void Disconnect()
		{
			foreach( ConnectionAsset connection in Connections )
			{
				RemoveConnection( connection );
			}
		}
		
		
		
		public virtual bool ContainsPoint( Vector3 point, Transform transform )
		{
			return false;
		}
		
		
		
		public bool ContainsPoint( Vector3 point )
		{
			if( Control.Instance == null || Control.Instance.Owner == null || Control.Instance.Owner.transform == null )
			{
				Debug.LogError( "Unable to access the current Control instance or its transform" );
				return false;
			}
			
			return ContainsPoint( point, Control.Instance.Owner.transform );
		}
		
		
		
		public virtual Vector3 GetNearestPoint( Vector3 point, float radius, Transform transform )
		{
			return Position;
		}
		
		
		
		public Vector3 GetNearestPoint( Vector3 point, float radius )
		{
			if( Control.Instance == null || Control.Instance.Owner == null || Control.Instance.Owner.transform == null )
			{
				Debug.LogError( "Unable to access the current Control instance or its transform" );
				return Position;
			}
			
			return GetNearestPoint( point, radius, Control.Instance.Owner.transform );
		}
		


		public virtual void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection )
		{
			Gizmos.color = ( !networkSelected ) ? Color.yellow : ( nodeSelected ) ? Color.white : Color.green;
		    Gizmos.DrawWireSphere( Position + transform.position + offset, standardMarkerSize );
			
			foreach( ConnectionAsset connection in Connections )
			{
				Vector3 from, to, vector;
				
				from = Position + transform.position + offset;
				to = connection.To.Position + transform.position + offset;
				vector = to - from;
				
				Gizmos.color = ( !networkSelected ) ? Color.yellow : ( selectedConnection == connection ) ? Color.white : Color.green;
					
				Gizmos.DrawLine( from + vector.normalized/* * waypoint.Radius*/,
					to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius */);
				
				Gizmos.DrawLine( to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius*/,
					to - vector.normalized * ( /*( connection.To as WaypointAsset ).Radius*/1.0f + connection.Width / 2.0f ) +
					Vector3.Lerp( ( Vector3.Cross( vector, Vector3.up ).normalized ),
					vector.normalized, 0.5f ) * connection.Width / 2.0f );
					
				Gizmos.DrawLine( to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius*/,
					to - vector.normalized * ( /*( connection.To as WaypointAsset ).Radius*/1.0f + connection.Width / 2.0f ) +
					Vector3.Lerp( ( Vector3.Cross( vector, Vector3.up ).normalized * -1.0f ),
					vector.normalized, 0.5f ) * connection.Width / 2.0f );
			}
		}



		public int CompareTo( object other )
		{
			NetworkNodeAsset otherNode;

			otherNode = other as NetworkNodeAsset;

			if( otherNode != null )
			{
				return Name.CompareTo( otherNode.Name );
			}
			else
			{
				throw new ArgumentException( "Cannot compare to non-node object" );
			}
		}
	}

}
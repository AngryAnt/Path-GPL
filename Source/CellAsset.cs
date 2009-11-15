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
	public class Edge
	{
		Vector3 a, b;
		
		
		
		public Edge( Vector3 a, Vector3 b )
		{
			this.a = a;
			this.b = b;
		}
		
		
		
		public Vector3 A
		{
			get
			{
				return a;
			}
		}
		
		
		
		public Vector3 B
		{
			get
			{
				return b;
			}
		}
		
		
		
		public bool Equals( Edge other )
		{
			return ( A == other.A && B == other.B ) || ( B == other.A && A == other.B );
		}
	}	

	[ Serializable() ]
	public class CellAsset : NetworkNodeAsset
	{
		private TriangleAsset[] triangles;
		//private Edge[] border;
		
		
		
		public CellAsset( string name, Vector3 position, TriangleAsset[] triangles, NetworkAsset network, CollectionAsset collection ) : base( network, collection )
		{
			this.Name = name;
			this.Position = position;
			
			foreach( TriangleAsset triangle in triangles )
			{
				triangle.Cell = this;
			}
			
			this.triangles = triangles;
			
			// TODO: Calculate border and position from triangles
			
			/*ArrayList edges, uses;
			Edge edge, existingEdge;
			
			edges = new ArrayList();
			uses = new ArrayList();
			
			foreach( TriangleAsset triangle in triangles )
			{
				for( int i = 0; i < 3; i++ )
				{
					edge = 	( i == 0 ) ? new Edge( triangle.Points[ 0 ], triangles.Points[ 1 ] ) :
							( i == 1 ) ? new Edge( triangle.Points[ 1 ], triangles.Points[ 2 ] ) :
							new Edge( triangle.Points[ 2 ], triangles.Points[ 0 ] );
							
					for( int j = 0; j < edges.Count; j++ )
					{
						existingEdge = edges[ j ];
						
						if( edge.Equals( existingEdge ) )
						{
							uses[ edges.IndexOf( existingEdge ) ]++;
						}
					}
				}
			}*/
		}
		
		
		
		public CellAsset( CellAsset original, NetworkAsset network ) : base( network, original.Collection )
		{
			this.Name = original.Name;
			this.Position = original.Position;
			
			this.triangles = new TriangleAsset[ original.Triangles.Length ];
			for( int i = 0; i < original.Triangles.Length; i++ )
			{
				this.triangles[ i ] = new TriangleAsset( original.Triangles[ i ], this );
			}
			
			this.Tags = new int[ original.Tags.Length ];
			for( int i = 0; i < original.Tags.Length; i++ )
			{
				this.Tags[ i ] = original.Tags[ i ];
			}
		}
		
		
		
		public TriangleAsset[] Triangles
		{
			get
			{
				return triangles;
			}
		}



		public override bool ContainsPoint( Vector3 point, Transform transform )
		{
			// TODO: Consider whether it would be useful to check if a radius can be contained within the triangles and if so, the case of a point which fits in the cell, but not in one single triangle should be considered
			foreach( TriangleAsset triangle in Triangles )
			{
				if( triangle.ContainsPoint( point, transform ) )
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		
		public override Vector3 GetNearestPoint( Vector3 point, float radius, Transform transform )
		{
			Vector3 nearest, current;
			
			if( Triangles[ 0 ].ContainsPoint( point, transform ) )
			{
				return point;
			}
			
			nearest = Triangles[ 0 ].GetNearestPoint( point, radius, transform );
			for( int i = 1; i < Triangles.Length; i++ )
			{
				if( Triangles[ i ].ContainsPoint( point, transform ) )
				{
					return point;
				}
				
				current = Triangles[ i ].GetNearestPoint( point, radius, transform );
				if( ( nearest - point ).magnitude > ( current - point ).magnitude )
				{
					nearest = current;
				}
			}
			
			return nearest;
		}



		public static CellAsset Merge( ArrayList cells )
		{
			CellAsset[] theCells;
			
			theCells = new CellAsset[ cells.Count ];
			
			for( int i = 0; i < cells.Count; i++ )
			{
				theCells[ i ] = ( CellAsset )cells[ i ];
			}
			
			return Merge( theCells );
		}



		public static CellAsset Merge( CellAsset[] cells )
		{
			CellAsset result;
			ArrayList tags;//, connections;
			TriangleAsset[] triangles;
			int triangleCount;
			
			if( cells.Length == 0 )
			{
				return null;
			}
			else if( cells.Length == 1 )
			{
				return cells[ 0 ];
			}
			
			triangleCount = 0;
			foreach( CellAsset cell in cells )
			{
				triangleCount += cell.Triangles.Length;
			}
			
			triangles = new TriangleAsset[ triangleCount ];
			
			triangleCount = 0;
			foreach( CellAsset cell in cells )
			{
				Array.Copy( cell.Triangles, 0, triangles, triangleCount, cell.Triangles.Length );
				triangleCount += cell.Triangles.Length;
			}
			
			tags = new ArrayList();
			
			foreach( CellAsset cell in cells )
			{
				foreach( int tag in cell.Tags )
				{
					if( !tags.Contains( tag ) )
					{
						tags.Add( tag );
					}
				}
			}
			
			result = new CellAsset( cells[ 0 ].Name, cells[ 0 ].Position, triangles, cells[ 0 ].Network, cells[ 0 ].Collection );

			foreach( int tag in tags )
			{
				result.AddTag( tag );
			}
			
			return result;
		}



		public override void OnRenderGizmos( Transform transform, float standardMarkerSize, Vector3 offset, bool networkSelected, bool nodeSelected, ConnectionAsset selectedConnection )
		{
			Gizmos.color = ( !networkSelected ) ? Color.yellow : ( nodeSelected ) ? Color.white : Color.green;
			
		    Gizmos.DrawWireSphere( Position + transform.position + offset, standardMarkerSize );
			
			foreach( TriangleAsset triangle in triangles )
			{
				Gizmos.DrawLine( triangle.Points[ 0 ] + transform.position + offset, triangle.Points[ 1 ] + transform.position + offset );
				Gizmos.DrawLine( triangle.Points[ 1 ] + transform.position + offset, triangle.Points[ 2 ] + transform.position + offset );
				Gizmos.DrawLine( triangle.Points[ 2 ] + transform.position + offset, triangle.Points[ 0 ] + transform.position + offset );
			}
			
			// TODO: Draw polygon border
			// [17:47]  <AngryAnt> Ah right. I'm just thinking in vertices because thats the info I have available. So in that context it becomes: If only one triangle contains both vertices of an edge then the edge is on the border of the polygon?
			
			/*foreach( ConnectionAsset connection in Connections )
			{
				Vector3 from, to, vector;
				
				from = Position + transform.position + offset;
				to = connection.To.Position + transform.position + offset;
				vector = to - from;
				
				Gizmos.color = ( !networkSelected ) ? Color.yellow : ( selectedConnection == connection ) ? Color.white : Color.green;
					
				Gizmos.DrawLine( from + vector.normalized/* * waypoint.Radius* /,
					to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius * /);
				
				Gizmos.DrawLine( to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius* /,
					to - vector.normalized * ( /*( connection.To as WaypointAsset ).Radius* /1.0f + /*connection.Width* /1.0f / 2.0f ) +
					Vector3.Lerp( ( Vector3.Cross( vector, Vector3.up ).normalized ),
					vector.normalized, 0.5f ) * /*connection.Width* /1.0f / 2.0f );
					
				Gizmos.DrawLine( to - vector.normalized/* * ( connection.To as WaypointAsset ).Radius* /,
					to - vector.normalized * ( /*( connection.To as WaypointAsset ).Radius* /1.0f + /*connection.Width* /1.0f / 2.0f ) +
					Vector3.Lerp( ( Vector3.Cross( vector, Vector3.up ).normalized * -1.0f ),
					vector.normalized, 0.5f ) * /*connection.Width* /1.0f / 2.0f );
			}*/
		}
	}
}
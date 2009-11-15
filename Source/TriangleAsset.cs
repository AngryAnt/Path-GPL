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
		public class TriangleAsset : ISerializable
		// TODO: This entire class needs a good clean
		{
			private Vector3[] points;
			private CellAsset cell;



			public TriangleAsset( Vector3[] points, CellAsset cell )
			{
				Points = points;
				this.cell = cell;
			}



			public TriangleAsset( TriangleAsset original, CellAsset cell )
			{
				this.Points = original.Points;
				this.cell = cell;
			}



			public TriangleAsset( SerializationInfo serializationInfo, StreamingContext streamingContext )
			{
				float[] x, y, z;

				x = serializationInfo.GetValue( "x", typeof( float[] ) ) as float[];
				y = serializationInfo.GetValue( "y", typeof( float[] ) ) as float[];
				z = serializationInfo.GetValue( "z", typeof( float[] ) ) as float[];
				Cell = serializationInfo.GetValue( "cell", typeof( CellAsset ) ) as CellAsset;

				Points = new Vector3[ 3 ];

				Points[ 0 ] = new Vector3( x[ 0 ], y[ 0 ], z[ 0 ] );
				Points[ 1 ] = new Vector3( x[ 1 ], y[ 1 ], z[ 1 ] );
				Points[ 2 ] = new Vector3( x[ 2 ], y[ 2 ], z[ 2 ] );
			}



			public void GetObjectData( SerializationInfo serializationInfo, StreamingContext streamingContext )
			{
				float[] x, y, z;

				x = new float[ 3 ];
				y = new float[ 3 ];
				z = new float[ 3 ];

				x[ 0 ] = Points[ 0 ].x;
				y[ 0 ] = Points[ 0 ].y;
				z[ 0 ] = Points[ 0 ].z;

				x[ 1 ] = Points[ 1 ].x;
				y[ 1 ] = Points[ 1 ].y;
				z[ 1 ] = Points[ 1 ].z;

				x[ 2 ] = Points[ 2 ].x;
				y[ 2 ] = Points[ 2 ].y;
				z[ 2 ] = Points[ 2 ].z;

				serializationInfo.AddValue( "x", x );
				serializationInfo.AddValue( "y", y );
				serializationInfo.AddValue( "z", z );
				serializationInfo.AddValue( "cell", cell );
			}



			public CellAsset Cell
			{
				get
				{
					return cell;
				}
				set
				{
					cell = value;
				}
			}



			public Vector3[] Points
			{
				get
				{
					return points;
				}
				set
				{
					points = value;
				}
			}



			public Vector3[] AbsolutePoints( Transform transform )
			{
				Vector3[] absolutePoints;

				absolutePoints = new Vector3[ 3 ];
				absolutePoints[ 0 ] = transform.TransformPoint( Points[ 0 ] + Cell.Network.Position );
				absolutePoints[ 1 ] = transform.TransformPoint( Points[ 1 ] + Cell.Network.Position );
				absolutePoints[ 2 ] = transform.TransformPoint( Points[ 2 ] + Cell.Network.Position );

				return absolutePoints;
			}


			public bool ContainsPoint( Vector3 point, Transform transform/*, float allowedEdgeDiff*/ )
			{
				Vector2 a, b, c, v0, v1, v2, p;
				float dot00, dot01, dot02, dot11, dot12, u, v, invDenom;
				Vector3[] absPoints;

				absPoints = AbsolutePoints( transform );

				a = new Vector2( absPoints[ 0 ].x, absPoints[ 0 ].z );
				b = new Vector2( absPoints[ 1 ].x, absPoints[ 1 ].z );
				c = new Vector2( absPoints[ 2 ].x, absPoints[ 2 ].z );
				p = new Vector2( point.x, point.z );

	/*
				// See if we're near enough one of the edges
				if( ( ( ClosestPointOnLine( a, b, point ) - point ).magnitude <= allowedEdgeDiff ) ||
				( ( ClosestPointOnLine( a, c, point ) - point ).magnitude <= allowedEdgeDiff ) ||
				( ( ClosestPointOnLine( b, c, point ) - point ).magnitude <= allowedEdgeDiff ) )
				{
					return true;
				}*/

				v0 = c - a;
				v1 = b - a;
				v2 = p - a;

				dot00 = Vector3.Dot( v0, v0 );
				dot01 = Vector3.Dot( v0, v1 );
				dot02 = Vector3.Dot( v0, v2 );
				dot11 = Vector3.Dot( v1, v1 );
				dot12 = Vector3.Dot( v1, v2 );

				invDenom = 1 / ( dot00 * dot11 - dot01 * dot01 );
				u = ( dot11 * dot02 - dot01 * dot12 ) * invDenom;
				v = ( dot00 * dot12 - dot01 * dot02 ) * invDenom;

				return ( u > 0 ) && ( v > 0 ) && ( u + v < 1 );
			}



			public Vector3 GetNearestPoint( Vector3 point, float clearRadius, Transform transform )
			{
				/*Vector3 distantCorner, nearPoint, cornerOne, cornerTwo;
				ArrayList tempCorners;
				bool firstRun;


				firstRun = true;
				distantCorner = Points[ 0 ];
				foreach( Vector3 corner in Points )
				// Find the corner the farthest away from our point
				{
					if( firstRun )
					// Skip first run as we already set distantCorner to the value of Points[ 0 ]
					{
						firstRun = false;
						continue;
					}

					if( ( distantCorner - point ).magnitude < ( corner - point ).magnitude )
					{
						distantCorner = corner;
					}
				}


				// That corner is not on the nearest edge, so we remove it //
				tempCorners = new ArrayList( Points );
				tempCorners.Remove( distantCorner );


				// Grab the nearest corners - the near edge ends //
				cornerOne = ( Vector3 )tempCorners[ 0 ];
				cornerTwo = ( Vector3 )tempCorners[ 1 ];


				/*if( ( cornerOne - cornerTwo ).magnitude < clearRadius * 2 )
				// The diameter of our unit will not fit through this edge
				{
					Debug.Log( "Raaaa!" );
					return point;
				}* /


				nearPoint = ClosestPointOnLine( cornerOne, cornerTwo, point );
					// Get the nearest point
				*/

				Vector3[] points, absPoints;
				Vector3 nearPoint;

				absPoints = AbsolutePoints( transform );
				points = new Vector3[ 3 ];
				points[ 0 ] = ClosestPointOnLine( absPoints[ 0 ], absPoints[ 1 ], point, clearRadius );
				points[ 1 ] = ClosestPointOnLine( absPoints[ 0 ], absPoints[ 2 ], point, clearRadius );
				points[ 2 ] = ClosestPointOnLine( absPoints[ 2 ], absPoints[ 1 ], point, clearRadius );

				//nearPoint = ( ( points[ 0 ] - point ).magnitude < ( points[ 1 ] - point ).magnitude ) ? points[ 0 ] : points[ 1 ];
				//nearPoint = ( ( nearPoint - point ).magnitude < ( points[ 2 ] - point ).magnitude ) ? nearPoint : points[ 2 ];
				nearPoint = NearestPointXZ( points[ 0 ], points[ 1 ], point );
				nearPoint = NearestPointXZ( nearPoint, points[ 2 ], point );

				/*if( ( cornerOne - nearPoint ).magnitude < clearRadius )
				// There is not enough space from corner one to fit given radius - push away
				{
					nearPoint = cornerTwo - cornerOne;
					nearPoint.Normalize();
					nearPoint = cornerOne + nearPoint * clearRadius;
				}
				else if( ( cornerTwo - nearPoint ).magnitude < clearRadius )
				// There is not enough space from corner two to fit given radius - push away
				{
					nearPoint = cornerOne - cornerTwo;
					nearPoint.Normalize();
					nearPoint = cornerTwo + nearPoint * clearRadius;
				}*/


				return nearPoint;
			}



			private Vector3 NearestPointXZ( Vector3 pointOne, Vector3 pointTwo, Vector3 destination )
			{
				Vector2 a, b, c;

				a = new Vector2( pointOne.x, pointOne.z );
				b = new Vector2( pointTwo.x, pointTwo.z );
				c = new Vector2( destination.x, destination.z );

				return ( ( a - c ).magnitude < ( b - c ).magnitude ) ? pointOne : pointTwo;
			}



			public Vector3 ClosestPointOnLine( Vector3 cornerOne, Vector3 cornerTwo, Vector3 point, float clearRadius )
			// TODO: Prettification please!
			{
				// Determine t (the length of the vector from 'a' to 'p')



				Vector2 a = new Vector2( cornerOne.x, cornerOne.z );
				Vector2 b = new Vector2( cornerTwo.x, cornerTwo.z );
				Vector2 p = new Vector2( point.x, point.z );
				Vector2 c = p - a;
				Vector2 V = b - a;
				Vector2 result;
				float d = V.magnitude;
				// V.Normalize();
				V /= V.magnitude;
				float t = Vector2.Dot( V, c );

				// Check to see if t is beyond the extents of the line segment
				if( t < 0 )
				{
					result = a;
				}
				else if( t > d )
				{
					result = b;
				}
		 		else
				// Return the point between a and b
				{
					V = V * t;

					// set length of V to t;
					result = a + V;
				}

				if( ( a - result ).magnitude < clearRadius )
				// There is not enough space from corner one to fit given radius - push away
				{
					result = b - a;
					result = a + /*result.normalized*/( result / result.magnitude ) * clearRadius;
				}
				else if( ( b - result ).magnitude < clearRadius )
				// There is not enough space from corner two to fit given radius - push away
				{
					result = a - b;
					result = b + /*result.normalized*/( result / result.magnitude ) * clearRadius;
				}

				return new Vector3( result.x, Mathf.Lerp( cornerOne.y, cornerTwo.y, ( b - a ).magnitude / ( p - a ).magnitude ) , result.y );
			}
		}

}
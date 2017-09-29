<p><strong>Fergo SimpleDXF</strong></p>
<p>Fergo SimpleDXF is a simple .NET (C#) DXF library for reading Autodesk DXF format. It's is not coded to be a complete and full-featured library for reading the entire DXF format, but to be a fast solution for those who just need the basic 2D geometry information (no Z axis). For example, it doesn't read tables (with the exception of the "layer" element), classes or header and also doesn't support linetypes, lineweight, dimension styles, text styles etc. It's mainly focused at the DXF entities.&nbsp;</p>
<p>Below is table containing the entities supported at the moment :</p>
<table border="0">
<tbody>
<tr>
<td style="width: 75px;"><strong>Entity</strong></td>
<td style="width: 500px;"><strong>Properties</strong></td>
</tr>
<tr>
<td>LAYER</td>
<td>Name, Color</td>
</tr>
<tr>
<td>LINE</td>
<td>Layer, Start Position, End Position</td>
</tr>
<tr>
<td>CIRCLE</td>
<td>Layer, Center Position, Radius</td>
</tr>
<tr>
<td>ARC</td>
<td>Layer, Center Position, Radius, Start Angle, End Angle</td>
</tr>
<tr>
<td>POINT</td>
<td>Layer, Position</td>
</tr>
<tr>
<td>TEXT</td>
<td>Layer, Position, Text</td>
</tr>
<tr>
<td>POLYLINE</td>
<td>Layer, Vertex List, Closed Flag</td>
</tr>
<tr>
<td>LWPOLYLINE</td>
<td>Layer, Vertex List, Closed Flag</td>
</tr>
<tr>
<td>VERTEX</td>
<td>Layer, Position, Bulge</td>
</tr>
</tbody>
</table>
<p>This library also has methods to generate polygonal vertexes, at a given precision, for curved shapes such as Circles, Arcs and Polylines. It currently doesn't support blocks, but I plan to add support for it in the future. At the moment, is best to work with DXF not containing any blocks (explode the entire drawing).</p>

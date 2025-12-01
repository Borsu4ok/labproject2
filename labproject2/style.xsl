<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes"/>
	<xsl:template match="/">
		<html>
			<head>
				<title>Library Report</title>
				<style>
					table { border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; }
					th, td { border: 1px solid #333; padding: 8px; }
					th { background-color: #f2f2f2; }
				</style>
			</head>
			<body>
				<h2>Library Books Report</h2>
				<table>
					<tr>
						<th>Category</th>
						<th>Title</th>
						<th>Author</th>
						<th>Reader Name</th>
					</tr>
					<xsl:for-each select="Library/Book">
						<tr>
							<td>
								<xsl:value-of select="@Category"/>
							</td>
							<td>
								<xsl:value-of select="Title"/>
							</td>
							<td>
								<xsl:value-of select="Author"/>
							</td>
							<td>
								<xsl:value-of select="Reader/Name"/>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
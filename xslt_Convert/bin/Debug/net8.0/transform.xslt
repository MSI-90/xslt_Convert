<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    
    <xsl:output method="xml" indent="yes" encoding="utf-8"/>
    
    <xsl:template match="/Pay">
        <Employees>
            <xsl:for-each select="item">
                <Employee name="{@name}" surname="{@surname}">
                    <xsl:attribute name="amount">
                        <xsl:value-of select="@amount"/>
                    </xsl:attribute>
                    <xsl:attribute name="month">
                        <xsl:value-of select="@mount"/>
                    </xsl:attribute>
                </Employee>
            </xsl:for-each>
        </Employees>
    </xsl:template>
    
</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="xml" indent="yes" encoding="utf-8"/>

    <!-- группировка -->
    <xsl:key name="empKey" match="item" use="concat(@name, @surname)" />

    <xsl:template match="/Pay">
        <Employees>

            <!-- сотрудники -->
            <xsl:for-each select="item[generate-id() = generate-id(key('empKey', concat(@name, @surname))[1])]">

                <Employee name="{@name}" surname="{@surname}">

                    <!-- все записи этого сотрудника -->
                    <xsl:for-each select="key('empKey', concat(@name, @surname))">
                        <amount salary="{@amount}" month="{@mount}" />
                    </xsl:for-each>

                </Employee>

            </xsl:for-each>

        </Employees>
    </xsl:template>

</xsl:stylesheet>
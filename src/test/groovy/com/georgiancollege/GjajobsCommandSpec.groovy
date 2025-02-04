package com.georgiancollege

import io.micronaut.configuration.picocli.PicocliRunner
import io.micronaut.context.ApplicationContext
import io.micronaut.context.env.Environment

import spock.lang.AutoCleanup
import spock.lang.Shared
import spock.lang.Specification

import java.io.ByteArrayOutputStream
import java.io.PrintStream

class GjajobsCommandSpec extends Specification {

    @Shared @AutoCleanup ApplicationContext ctx = ApplicationContext.run(Environment.CLI, Environment.TEST)

    void "test gjajobs with command line option"() {
        given:
        ByteArrayOutputStream baos = new ByteArrayOutputStream()
        System.setOut(new PrintStream(baos))

        String[] args = ['--job', 'testJob', '--action', 'submit', '--jobType', 'testType', '--user', 'testUser', '--pwd', 'testPwd', '--seqno', '1', '--printer', 'testPrinter', '--formName', 'testForm', '--submitTime', 'now', '--completionTime', 'later'] as String[]
        PicocliRunner.run(GjajobsCommand, ctx, args)

        expect:
        baos.toString().equals('')
    }
}


# Deployment to AWS

We will use AWS CloudFormation to deploy the EC2 instance. This will create the necessary resources and
network configuration to run ElasticSearch on AWS. The CloudFormation template is [here](template.yaml).

Once you have the instance running, you can ssh into it and follow the instructions below to set up 
ElasticSearch using docker. This will first require mounting the EBS volume to the EC2 instance and
configuring the docker daemon to use it.

## Deploy the EC2 instance with CloudFormation

To integrate with other systems, this elastic search instance must be deployed to AWS. Using
the AWS CloudFormation template, the necessary resources can be created as follows:

```bash
aws cloudformation create-stack \
  --stack-name doyen-es \
  --template-body file:///full/path/to/doyen/ingestion_pipeline/aws_deployment/template.yaml \
  --parameters ParameterKey=MyHomeIp,ParameterValue=<your ip> \
               ParameterKey=MyKeyPairName,ParameterValue=<your key pair name>
```
Note that you have to give your local IP address, including the cidr block (e.g. 123.45.67.8/32). This will (at 
least initially) restrict access to the EC2 instance to your home network. Similarly, give the name of an existing EC2
key pair that you have access to. This will set up ssh credentials for the EC2 instance.

You can monitor the progress of the deployment using

```bash
aws cloudformation describe-stacks --stack-name doyen-es --query 'Stacks[0].StackStatus'
```

If there is an error, you can examine the logs using

```bash
aws cloudformation describe-stack-events --stack-name doyen-es
```

You can update the stack using the same command as above, but with `update-stack` instead of `create-stack`:

```bash
aws cloudformation update-stack \
  --stack-name doyen-es \
  --template-body file:///full/path/to/doyen/ingestion_pipeline/aws_deployment/template.yaml \
  --parameters ParameterKey=MyHomeIp,ParameterValue=<your ip> \
               ParameterKey=MyKeyPairName,ParameterValue=<your key pair name>
```

## Setting up Docker

Once the EC2 instance is running, you can ssh into it using the key pair you specified above:

```bash
ssh -i <path to private key> ec2-user@<public ip address>
```

Once you have ssh'd into the EC2 instance, you will need to install docker. You can do this as follows:

```bash
sudo amazon-linux-extras install docker
```

You will need to mount the EBS volume to the EC2 instance. First, you will need to find the volume in
/dev. You can do this using `lsblk`:

```bash
lsblk --output NAME,FSTYPE,SIZE,MOUNTPOINT
```

This should produce something like:

```
NAME          FSTYPE  SIZE MOUNTPOINT
nvme1n1               100G 
nvme0n1                 8G 
├─nvme0n1p1   xfs       8G /
└─nvme0n1p128           1M 
```

Note the much larger volume that is not mounted or formatted. If you see that FSTYPE is xfs and that
there is already a mount point, you're all set and can skip ahead to Configuring Docker.


### Formatting and mounting the EBS volume

You will first need to format it as xfs:

```bash
sudo mkfs -t xfs /dev/nvme1n1
```

Next, you will want to create the directory where you will mount the volume, and then mount it:

```bash
sudo mkdir /volume
sudo mount /dev/nvme1n1 /volume
```


### Configuring Docker to use the EBS volume

First, stop the docker daemon:

```bash
sudo service docker stop
```

Next, you will need to configure the docker daemon to use the EBS volume. To do this, you will need to
create a file at `/etc/docker/daemon.json` with the following contents using your favorite text editor:
```json
{
  "data-root": "/volume/docker"
}
```
You will likely need to use `sudo` to edit the file. Once you have done this, you will need to relocate
any existing docker images and containers to the new location. You can do this as follows, also renaming
the old docker directory as an extra precaution:

```bash
sudo mv /var/lib/docker/* /volume/docker
sudo mv /var/lib/docker /var/lib/docker.bak
```

Finally, you can start the docker daemon:

```bash
sudo service docker start
```


## Setting up ElasticSearch

You are now free to set up ElasticSearch as shown in the [ingestion README](../README.md), with a single modification:
when you run the docker container, you will need to add two environment variables to the command:

```bash
... --env "discovery.type=single-node" --env "network.host=0.0.0.0" ...
```

This will allow the container to be accessed from outside the EC2 instance. You can now access the ElasticSearch
instance from your local machine using the public IP address of the EC2 instance.

I also suggest using volumes to persist the data in ElasticSearch. This will allow you to stop and start the
container without losing data. You can do this by adding the following to the docker command:

```bash
... --volume /volume/esdata:/usr/share/elasticsearch/data ...
```


## Installing the Ingestion Pipeline

Make sure python is set up with at least 3.7. You can install pip as shown
[here](https://pip.pypa.io/en/stable/installing/). The best way to install the ingestion pipeline is to `pip` install
it from the GitHub repository:

```bash
pip install "git++https://github.com/DoyenTeam/doyen.git@main#egg=doyen_ingestion&subdirectory=ingestion_pipeline"
``` 

This assumes the current version you want to install is on "main". You can also install a different branch
by replacing "main" with the name of the branch. You can also install a specific commit by replacing "main"
with the commit hash.

In practice, I found that if I wanted to update the ingestion pipeline, I had to uninstall it first, then
reinstall it.

Once it is installed, you should be able to run:

```bash
doyen-ingest --help
```

and get a help message without errors. You may get a message prompting you to fill out a config file in 
`~/.doyen/config.ini`. See the [ingestion README](../README.md) for details on how to fill out this file.


## Set up Updates

You can set up regular updates of the ES content using `cron`. To do this, you will need to create a cron job
using `crontab -e`. You will need to add the following line to the file:

```bash
0 23 * * 6 /home/ec2-user/.venvs/doyen/bin/python -m doyen_ingestion.pubmed_processor -s -365 -y -10 
```

where it is assumed the virtual environment is located at `/home/ec2-user/.venvs/doyen`. This will run the
pipeline every Saturday at 11pm, rebuilding the index from the latest 365 files, including content up to 10
years old. Note that 365 is not a particularly special number, but was rather based on the fact that at the
time of recording there were 165 update files.


## A Final Note the use of AI Tools

This section of the project made extensive use of AI tools. In particular, the following tools were used:
    - [GitHub Copilot](https://copilot.github.com/)
    - [ChatGPT](https://chat.openai.com/)

ChatGPT was used on a free trial account and GitHub Copilot was used on a paid account integrated with
Pycharm. ChatGPT was used extensively when formulating the CloudFormation template. I used to to generate
chunks of the template, and then asked questions to help me fix it and fill in the gaps. GitHub Copilot offered
suggestions for completions in the template, both for comments and for resource definitions. The process was
altogether highly integrated and iterative. I would get a suggestion from ChatGPT, apply it with some completion
and input from GitHub Copilot, try to run the template, then start the cycle over again to refine the template
based on the results. Some old-fashioned googling was also used to verify the results given by ChatGPT and
debug some specific errors that ChatGPT and Copilot struggled with.

I found that ChatGPT was extremely valuable in this particular domain. Given the highly structured nature of
CloudFormation templates, it was able to generate large chunks of the template, in particular providing
correct boilerplate for each resource definition. The alternative method requires extensive manual work searching
through the AWS documentation and piecing together the template. Although not conceptually complex, every detail
of spelling matters, and it is easy to make a mistake. In the same way, GitHub Copilot was also extremely valuable
when I wanted to make minor adjustments to the template. 

My primary goal in using AI for this task was to evaluate its effectiveness and learn how to effectively use it. I
found this niche to be a perfect fit for AI tools. The task was highly structured, but also required a lot of
manual lookup of arbitrary names and boilerplate. In addition, although finding the exact boilerplate is hard,
it is easy for a human to parse its meaning. This combination of hard-to-form but easy-to-check task is perfect for
AI. Incidentally, it is also some of the most tedious work in software development.

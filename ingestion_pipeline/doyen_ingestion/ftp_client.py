import ftplib
import logging
import xml.etree.ElementTree as ET
import zlib
from contextlib import contextmanager
from io import BytesIO
from pathlib import Path
from typing import List, Tuple, Union

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

FTP_BLOCK_SIZE = 33554432  # Chunk size recommended by NCBI


@contextmanager
def ftp_connection(ftp_url: str):
    """Get an FTP connection for the given URL and login."""
    ftp_session = ftplib.FTP(ftp_url)
    ftp_session.login()
    yield ftp_session


class NihFtpClient(object):
    """High level access to the NIH FTP repositories.

    Parameters
    ----------
    root : Path
        The path to the subdirectory around which this client operates.
    """

    ftp_url = "ftp.ncbi.nlm.nih.gov"

    def __init__(self, root: Union[Path, str]):
        if not isinstance(root, Path):
            root = Path(root)
        self.root = root
        return

    def get_xml_tree(self, file_name):
        """Get the content from an xml file as an ElementTree."""
        logger.info(f"Downloading {file_name}")
        xml_bytes = self.get_file(file_name, force_str=False, decompress=True)
        logger.info("Parsing XML metadata")
        return ET.XML(xml_bytes)

    def download_file(self, file_path: Union[Path, str], dest_file: Path = None):
        """Download a file into a file given by file_path."""
        if not dest_file:
            dest_file = Path("..") / file_path.name

        full_path = self.root / file_path
        logger.info(full_path)
        with dest_file.open("wb") as gzf:
            with ftp_connection(self.ftp_url) as ftp:
                ftp.retrbinary(
                    f"RETR {full_path.as_posix()}",
                    callback=lambda s: gzf.write(s),
                    blocksize=FTP_BLOCK_SIZE,
                )
                gzf.flush()
        return

    def get_file(self, file_path: Union[Path, str], force_str=True, decompress=True):
        """Get the contents of a file as a string."""

        full_path = self.root / file_path
        gzf_bytes = BytesIO()
        with ftp_connection(self.ftp_url) as ftp:
            ftp.retrbinary(
                f"RETR {full_path.as_posix()}",
                callback=lambda s: gzf_bytes.write(s),
                blocksize=FTP_BLOCK_SIZE,
            )
            gzf_bytes.flush()
        ret = gzf_bytes.getvalue()

        if file_path.endswith(".gz") and decompress:
            ret = zlib.decompress(ret, 16 + zlib.MAX_WBITS)

        if force_str and isinstance(ret, bytes):
            ret = ret.decode("utf8")

        return ret

    def list(
        self, dir_path: Union[Path, str] = None, with_timestamps=True
    ) -> List[Tuple[str, int]]:
        """List all contents the ftp directory."""
        if dir_path is None:
            dir_path = self.root
        else:
            dir_path = self.root / dir_path

        with ftp_connection(self.ftp_url) as ftp:
            if with_timestamps:
                raw_contents = ftp.mlsd(dir_path.as_posix())
                contents = [
                    (k, meta["modify"])
                    for k, meta in raw_contents
                    if not k.startswith(".")
                ]
            else:
                contents = ftp.nlst()
        return contents
